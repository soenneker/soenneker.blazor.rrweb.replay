export function createInterop() {
    const players = new Map();
    let disposed = false;

    function get(id) {
        if (disposed) throw new Error("The replay service has been disposed.");
        const entry = players.get(id);
        if (!entry) throw new Error(`Unknown rrweb replay: ${id}`);
        return entry;
    }

    function validateEvent(event) {
        if (!event || !Number.isInteger(event.type) || !Number.isFinite(event.timestamp) || !("data" in event))
            throw new Error("Each rrweb event must contain type, timestamp, and data.");
    }

    function validateOffset(offset) {
        if (offset != null && (!Number.isFinite(offset) || offset < 0))
            throw new Error("Replay offsets must be finite and nonnegative.");
    }

    function destroy(id) {
        const entry = players.get(id);
        if (!entry) return;
        try { entry.stopFitting?.(); }
        finally {
            try { entry.player.destroy(); }
            finally { players.delete(id); }
        }
    }

    function setFitToContainer(id, enabled) {
        const entry = get(id);
        entry.stopFitting?.();
        entry.stopFitting = null;
        if (!enabled) return;
        const host = entry.root;
        const replay = host.querySelector('.replayer-wrapper');
        const iframe = replay?.querySelector('iframe');
        if (!iframe) throw new Error('The replay viewport is not available.');
        const hostProperties = ['height', 'overflow', 'minWidth', 'width'];
        const replayProperties = ['transform', 'transformOrigin', 'marginLeft', 'width'];
        const hostStyles = hostProperties.map(property => host.style[property]);
        const replayStyles = replayProperties.map(property => replay.style[property]);
        let frame = 0;
        host.style.overflow = 'hidden';
        host.style.minWidth = '0';
        host.style.width = '100%';
        const fit = () => {
            frame = 0;
            const width = iframe.offsetWidth;
            const height = iframe.offsetHeight;
            const available = host.clientWidth;
            if (!width || !height || !available) return;
            const scale = Math.min(1, available / width);
            replay.style.width = `${width}px`;
            replay.style.transformOrigin = 'top left';
            replay.style.transform = `scale(${scale})`;
            replay.style.marginLeft = `${Math.max(0, (available - width * scale) / 2)}px`;
            host.style.height = `${height * scale}px`;
        };
        const observer = new ResizeObserver(() => {
            if (!frame) frame = requestAnimationFrame(fit);
        });
        entry.stopFitting = () => {
            observer.disconnect();
            cancelAnimationFrame(frame);
            hostProperties.forEach((property, index) => { host.style[property] = hostStyles[index]; });
            replayProperties.forEach((property, index) => { replay.style[property] = replayStyles[index]; });
        };
        try {
            observer.observe(host);
            observer.observe(iframe);
            fit();
        } catch (error) {
            entry.stopFitting();
            entry.stopFitting = null;
            throw error;
        }
    }

    return {
        create(id, root, eventsJson, optionsJson) {
            if (disposed) throw new Error("The replay service has been disposed.");
            if (!id || players.has(id)) throw new Error("A unique replay ID is required.");
            if (!root || root.nodeType !== 1 || !root.isConnected) throw new Error("A rendered replay host element is required.");
            const Replayer = globalThis.rrwebReplay?.Replayer;
            if (typeof Replayer !== "function") throw new Error("rrweb replay resources are not loaded.");
            const events = JSON.parse(eventsJson);
            const options = JSON.parse(optionsJson);
            if (!Array.isArray(events) || (!options.liveMode && events.length < 2))
                throw new Error("Replay requires at least two recorded events.");
            if (!Number.isFinite(options.speed) || options.speed <= 0) throw new Error("Replay speed must be positive.");
            events.forEach(validateEvent);
            events.sort((a, b) => a.timestamp - b.timestamp);
            const player = new Replayer(events, { ...options, root });
            players.set(id, { player, root, liveMode: !!options.liveMode, stopFitting: null });
        },
        play(id, offset) {
            validateOffset(offset);
            const { player } = get(id);
            // rrweb's baseline is zero before the first play, yielding a negative current time.
            player.play(offset ?? Math.max(0, player.getCurrentTime()));
        },
        pause(id, offset) {
            validateOffset(offset);
            get(id).player.pause(offset ?? undefined);
        },
        setSpeed(id, speed) {
            if (!Number.isFinite(speed) || speed <= 0) throw new Error("Replay speed must be positive.");
            get(id).player.setConfig({ speed });
        },
        setFitToContainer,
        getCurrentTime(id) { return Math.max(0, get(id).player.getCurrentTime()); },
        getDuration(id) { return get(id).player.getMetaData().totalTime; },
        addEvent(id, eventJson) {
            const event = JSON.parse(eventJson);
            validateEvent(event);
            get(id).player.addEvent(event);
        },
        startLive(id, baselineTime) {
            validateOffset(baselineTime);
            const { player, liveMode } = get(id);
            if (!liveMode) throw new Error("The replayer must be created with LiveMode enabled.");
            player.startLive(baselineTime ?? undefined);
        },
        destroy,
        dispose() {
            if (disposed) return;
            const errors = [];
            for (const id of players.keys()) {
                try { destroy(id); } catch (error) { errors.push(error); }
            }
            disposed = true;
            if (errors.length) throw new AggregateError(errors, "Failed to destroy rrweb replayers.");
        }
    };
}

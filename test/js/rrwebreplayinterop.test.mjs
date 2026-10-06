import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/Soenneker.Blazor.Rrweb.Replay/wwwroot/js/rrwebreplayinterop.js', import.meta.url), 'utf8');
const { createInterop } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
const events = JSON.stringify([{ type: 2, data: {}, timestamp: 200 }, { type: 4, data: {}, timestamp: 100 }]);
const root = { nodeType: 1, isConnected: true };
const options = JSON.stringify({ speed: 1 });

function setup() {
    const instances = [];
    globalThis.rrwebReplay = { Replayer: class {
        constructor(events, options) { this.events = events; this.options = options; this.time = -100; instances.push(this); }
        play(offset) { this.time = offset; }
        pause(offset) { if (offset !== undefined) this.time = offset; }
        setConfig(config) { Object.assign(this.options, config); }
        getCurrentTime() { return this.time; }
        getMetaData() { return { totalTime: 100 }; }
        addEvent(event) { this.events.push(event); }
        startLive(baseline) { this.baseline = baseline; }
        destroy() { this.destroyed = true; }
    } };
    return instances;
}

test('first play starts at zero; resume preserves position and explicit zero seeks', () => {
    const instances = setup();
    const replay = createInterop();
    replay.create('one', root, events, options);
    assert.equal(replay.getCurrentTime('one'), 0);
    replay.play('one', null);
    assert.equal(instances[0].time, 0);
    replay.pause('one', 50);
    replay.play('one', null);
    assert.equal(instances[0].time, 50);
    replay.play('one', 0);
    assert.equal(instances[0].time, 0);
    assert.deepEqual(instances[0].events.map(e => e.timestamp), [100, 200]);
    replay.dispose();
});

test('independent instances and destruction do not affect other players', () => {
    const instances = setup();
    const replay = createInterop();
    replay.create('one', root, events, options);
    replay.create('two', root, events, options);
    assert.throws(() => replay.create('one', root, events, options), /unique/);
    replay.setSpeed('two', 2);
    replay.destroy('one');
    replay.destroy('one');
    assert.equal(instances[0].destroyed, true);
    assert.equal(instances[1].destroyed, undefined);
    assert.equal(instances[1].options.speed, 2);
    replay.dispose();
    assert.equal(instances[1].destroyed, true);
    assert.throws(() => replay.play('two'), /disposed/);
});

test('live mode accepts an empty initial stream and validates appended events', () => {
    const instances = setup();
    const replay = createInterop();
    replay.create('live', root, '[]', '{"speed":1,"liveMode":true}');
    replay.startLive('live', 100);
    replay.addEvent('live', '{"type":4,"data":{},"timestamp":110}');
    assert.equal(instances[0].baseline, 100);
    assert.equal(instances[0].events.length, 1);
    assert.throws(() => replay.addEvent('live', '{}'), /Each rrweb event/);
    replay.dispose();
});

test('invalid hosts, recordings, offsets, and speeds are rejected', () => {
    setup();
    const replay = createInterop();
    assert.throws(() => replay.create('one', null, events, options), /host/);
    assert.throws(() => replay.create('one', root, '[]', options), /two recorded events/);
    replay.create('one', root, events, options);
    assert.throws(() => replay.startLive('one'), /LiveMode/);
    assert.throws(() => replay.play('one', -1), /nonnegative/);
    assert.throws(() => replay.setSpeed('one', Infinity), /positive/);
    assert.throws(() => replay.pause('missing'), /Unknown/);
    replay.dispose();
});

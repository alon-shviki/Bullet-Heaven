self.onmessage = function (e) {
    if (e.data?.type === 'handshake') {
        self.postMessage({ type: 'handshake-ack', ok: true });
        return;
    }
    // WORK-002 adds physics dispatch here
};

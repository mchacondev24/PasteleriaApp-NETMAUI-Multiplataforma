// PasteleriaApp Client Interop Utilities
window.pasteleriaApp = {
    playBeep: function () {
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(1200, ctx.currentTime);
            gain.gain.setValueAtTime(0.2, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.1);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start();
            osc.stop(ctx.currentTime + 0.1);
        } catch (e) {
            console.log('Audio beep fallback', e);
        }
    },

    setTheme: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
    },

    printElement: function (elementId) {
        const elem = document.getElementById(elementId);
        if (!elem) return;
        const win = window.open('', '_blank', 'width=400,height=600');
        win.document.write('<html><head><title>Ticket Impresión Pastelería</title><style>body{font-family:monospace;font-size:12px;white-space:pre;margin:10px;}</style></head><body>');
        win.document.write(elem.innerText);
        win.document.write('</body></html>');
        win.document.close();
        win.focus();
        win.print();
        win.close();
    },

    downloadTextFile: function (filename, text) {
        const element = document.createElement('a');
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        element.setAttribute('download', filename);
        element.style.display = 'none';
        document.body.appendChild(element);
        element.click();
        document.body.removeChild(element);
    }
};

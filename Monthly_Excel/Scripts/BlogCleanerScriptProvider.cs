namespace Monthly_Excel.Scripts
{
    public static class BlogCleanerScriptProvider
    {
        public static string GetEnableRightClickScript()
        {
            return @"
(() => {
    function enableForDocument(doc) {
        if (!doc) return;

        try {
            const blocker = (e) => {
                e.stopImmediatePropagation();
            };

            [
                'contextmenu',
                'selectstart',
                'dragstart',
                'copy',
                'cut',
                'paste',
                'mousedown',
                'mouseup',
                'keydown'
            ].forEach(type => {
                doc.addEventListener(type, blocker, true);
            });

            if (doc.body) {
                doc.body.oncontextmenu = null;
                doc.body.onselectstart = null;
                doc.body.ondragstart = null;
                doc.body.oncopy = null;
                doc.body.oncut = null;
                doc.body.onpaste = null;
                doc.body.onmousedown = null;
                doc.body.onmouseup = null;
            }

            doc.oncontextmenu = null;
            doc.onselectstart = null;
            doc.ondragstart = null;
            doc.oncopy = null;
            doc.oncut = null;
            doc.onpaste = null;
            doc.onmousedown = null;
            doc.onmouseup = null;

            const all = doc.querySelectorAll('*');

            all.forEach(el => {
                el.oncontextmenu = null;
                el.onselectstart = null;
                el.ondragstart = null;
                el.oncopy = null;
                el.oncut = null;
                el.onpaste = null;
                el.onmousedown = null;
                el.onmouseup = null;

                if (el.style) {
                    el.style.userSelect = 'text';
                    el.style.webkitUserSelect = 'text';
                    el.style.msUserSelect = 'text';
                    el.style.webkitTouchCallout = 'default';
                }
            });

            const style = doc.createElement('style');
            style.innerHTML = `
                * {
                    -webkit-user-select: text !important;
                    user-select: text !important;
                    -webkit-touch-callout: default !important;
                }
            `;
            (doc.head || doc.documentElement).appendChild(style);
        }
        catch (err) {
        }
    }

    function runAll() {
        enableForDocument(document);

        document.querySelectorAll('iframe, frame').forEach(frame => {
            try {
                if (frame.contentDocument) {
                    enableForDocument(frame.contentDocument);
                }
            }
            catch (err) {
            }
        });
    }

    runAll();

    let count = 0;
    const timer = setInterval(() => {
        runAll();
        count++;

        if (count >= 20) {
            clearInterval(timer);
        }
    }, 1000);

    return 'right-click force enabled';
})();
";
        }

        public static string GetCleanScript()
        {
            return @"
(() => {
    let frame = document.querySelector('#mainFrame');
    let doc = frame ? frame.contentDocument : document;

    let container =
        doc.querySelector('.se-main-container') ||
        doc.querySelector('.post-view') ||
        doc.body;

    if (!container) {
        return 'container not found';
    }

    container.innerHTML = container.innerHTML.replace(/\u200B/g, '');

    container.querySelectorAll('img').forEach(e => e.remove());
    container.querySelectorAll('br').forEach(e => e.remove());

    container.querySelectorAll('p').forEach(p => {
        if (!p.innerText.replace(/\u200B/g, '').trim()) {
            p.remove();
        }
    });

    return 'clean done';
})();
";
        }
    }
}
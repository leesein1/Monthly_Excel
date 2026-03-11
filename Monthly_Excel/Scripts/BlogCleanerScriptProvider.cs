using System;

namespace Monthly_Excel.Scripts
{
    public static class BlogCleanerScriptProvider
    {
        public static string GetEnableRightClickScript()
        {
            return @"
(() => {
    if (window.__blogCleanerRightClickApplied) return;
    window.__blogCleanerRightClickApplied = true;

    function enableForDocument(doc) {
        if (!doc) return;

        try {
            const stopBlocker = (e) => {
                e.stopImmediatePropagation();
            };

            [
                'contextmenu',
                'selectstart',
                'dragstart',
                'copy',
                'cut',
                'paste'
            ].forEach(type => {
                doc.addEventListener(type, stopBlocker, true);
            });

            const allowContextMenu = (e) => {
                e.stopPropagation();
            };

            doc.addEventListener('contextmenu', allowContextMenu, false);

            if (doc.body) {
                doc.body.oncontextmenu = null;
                doc.body.onselectstart = null;
                doc.body.ondragstart = null;
                doc.body.oncopy = null;
                doc.body.oncut = null;
                doc.body.onpaste = null;
                doc.body.style.userSelect = 'text';
                doc.body.style.webkitUserSelect = 'text';
                doc.body.style.msUserSelect = 'text';
                doc.body.style.webkitTouchCallout = 'default';
            }

            const all = doc.querySelectorAll('*');
            for (const el of all) {
                try {
                    el.oncontextmenu = null;
                    el.onselectstart = null;
                    el.ondragstart = null;
                    el.oncopy = null;
                    el.oncut = null;
                    el.onpaste = null;
                    if (el.style) {
                        el.style.userSelect = 'text';
                        el.style.webkitUserSelect = 'text';
                        el.style.msUserSelect = 'text';
                        el.style.webkitTouchCallout = 'default';
                    }
                } catch (_) {}
            }
        } catch (_) {}
    }

    function tryApply(doc) {
        try {
            enableForDocument(doc);
        } catch (_) {}
    }

    tryApply(document);

    const observer = new MutationObserver(() => {
        tryApply(document);
    });

    try {
        observer.observe(document.documentElement || document, {
            childList: true,
            subtree: true
        });
    } catch (_) {}

    try {
        const frames = document.querySelectorAll('iframe, frame');
        frames.forEach(frame => {
            try {
                const frameDoc = frame.contentDocument;
                if (frameDoc) enableForDocument(frameDoc);
            } catch (_) {}
        });
    } catch (_) {}
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

    if (!container) return 'no container';

    function isMeaningfulElement(el) {
        if (!el) return false;

        try {
            if (el.querySelector('img, video, iframe, frame, table, ul, ol, li, blockquote')) {
                return true;
            }

            const text = (el.innerText || el.textContent || '')
                .replace(/\u200B/g, '')
                .replace(/\u00A0/g, ' ')
                .trim();

            return text.length > 0;
        } catch (_) {
            return false;
        }
    }

    function removeIfEmptyChain(startEl) {
        let current = startEl;

        while (current && current !== container) {
            try {
                const hasElementChildren = current.children && current.children.length > 0;
                const text = ((current.innerText || current.textContent || '')
                    .replace(/\u200B/g, '')
                    .replace(/\u00A0/g, ' ')
                    .trim());

                if (!hasElementChildren && !text) {
                    const parent = current.parentElement;
                    current.remove();
                    current = parent;
                    continue;
                }
            } catch (_) {}

            break;
        }
    }

    // zero width space 제거
    try {
        container.innerHTML = container.innerHTML.replace(/\u200B/g, '');
    } catch (_) {}

    // 이미지 제거
    try {
        container.querySelectorAll('img').forEach(e => e.remove());
    } catch (_) {}

    // br 제거
    try {
        container.querySelectorAll('br').forEach(e => e.remove());
    } catch (_) {}

    // 네이버 링크 카드/링크 관련 요소 제거
    try {
        container.querySelectorAll(
            '.se-oglink-info, .__se_link, .se-oglink, .se-module-oglink, [data-linkdata]'
        ).forEach(e => {
            const parent = e.parentElement;
            e.remove();
            if (parent) removeIfEmptyChain(parent);
        });
    } catch (_) {}

    // iframe / frame 제거 + 비어버린 부모 정리
    try {
        const frameNodes = Array.from(container.querySelectorAll('iframe, frame'));
        frameNodes.forEach(el => {
            const parent = el.parentElement;
            el.remove();
            if (parent) removeIfEmptyChain(parent);
        });
    } catch (_) {}

    // CSS 찌꺼기 텍스트만 제거 (style 속성은 건드리지 않음)
    try {
        const cssTrashPattern = /^\s*(user-select\s*:\s*text;?|[-a-z]+user-select\s*:\s*text;?|webkit-touch-callout\s*:\s*default;?)\s*$/i;

        const walker = doc.createTreeWalker(
            container,
            NodeFilter.SHOW_TEXT,
            null
        );

        const textNodes = [];
        let node;

        while ((node = walker.nextNode())) {
            if (cssTrashPattern.test((node.textContent || '').trim())) {
                textNodes.push(node);
            }
        }

        textNodes.forEach(n => {
            const parent = n.parentElement;
            n.remove();
            if (parent) removeIfEmptyChain(parent);
        });
    } catch (_) {}

    // 완전히 빈 태그 정리
    try {
        const candidates = Array.from(container.querySelectorAll('p, div, span'));

        candidates.reverse().forEach(el => {
            try {
                if (isMeaningfulElement(el)) return;

                const hasChildren = el.children && el.children.length > 0;
                const text = ((el.innerText || el.textContent || '')
                    .replace(/\u200B/g, '')
                    .replace(/\u00A0/g, ' ')
                    .trim());

                if (!hasChildren && !text) {
                    el.remove();
                }
            } catch (_) {}
        });
    } catch (_) {}

    return 'cleaned';
})();
";
        }

        public static string GetCollectImageInfosScript()
        {
            return """
(() => {
    function getAccessibleDocuments(rootDoc) {
        const docs = [];
        const visited = new Set();

        function walk(doc) {
            if (!doc || visited.has(doc)) return;
            visited.add(doc);
            docs.push(doc);

            try {
                const frames = doc.querySelectorAll('iframe, frame');
                frames.forEach((frame) => {
                    try {
                        const frameDoc = frame.contentDocument;
                        if (frameDoc) walk(frameDoc);
                    } catch (_) {}
                });
            } catch (_) {}
        }

        try {
            const mainFrame = rootDoc.querySelector('#mainFrame');
            if (mainFrame && mainFrame.contentDocument) {
                walk(mainFrame.contentDocument);
            }
        } catch (_) {}

        walk(rootDoc);
        return docs;
    }

    function getContentRoots(doc) {
        const roots = [];

        try {
            const main = doc.querySelector('.se-main-container');
            if (main) roots.push(main);
        } catch (_) {}

        try {
            const oldView = doc.querySelector('.post-view');
            if (oldView && !roots.includes(oldView)) roots.push(oldView);
        } catch (_) {}

        try {
            const legacy = doc.querySelector('[id^="post-view"]');
            if (legacy && !roots.includes(legacy)) roots.push(legacy);
        } catch (_) {}

        return roots;
    }

    function toAbsoluteUrl(rawUrl, baseDoc) {
        if (!rawUrl) return '';

        try {
            if (rawUrl.startsWith('/')) {
                return new URL(rawUrl, baseDoc.location.origin).toString();
            }

            if (!rawUrl.startsWith('http')) {
                return new URL(rawUrl, baseDoc.location.href).toString();
            }
        } catch (_) {}

        return rawUrl;
    }

    function normalizeImageUrl(rawUrl, baseDoc) {
        let imageUrl = toAbsoluteUrl(rawUrl, baseDoc);
        if (!imageUrl) return '';

        imageUrl = imageUrl.replace(/[?&]photoView=[^&]*/g, '');

        try {
            const parsed = new URL(imageUrl);
            if (/(postfiles|blogfiles|post-phinf)\.pstatic\.net$/i.test(parsed.hostname)) {
                parsed.searchParams.delete('w');
                parsed.searchParams.delete('h');
                parsed.searchParams.set('type', 'w3840');
                imageUrl = parsed.toString();
            } else if (/blogthumb\.pstatic\.net$/i.test(parsed.hostname)) {
                parsed.searchParams.set('type', 'w3840');
                imageUrl = parsed.toString();
            } else if (/blog\.kakao|tistory/i.test(parsed.hostname)) {
                if (!parsed.searchParams.has('s') && !parsed.searchParams.has('w')) {
                    parsed.searchParams.set('s', 'l');
                }
                imageUrl = parsed.toString();
            } else {
                imageUrl = parsed.toString();
            }
        } catch (_) {}

        return imageUrl;
    }

    function parseLinkData(rawData) {
        if (!rawData) return null;
        try {
            return JSON.parse(rawData);
        } catch (_) {
            return null;
        }
    }

    function isMeaningfulContentImage(img, root) {
        if (!img || !root || !root.contains(img)) return false;

        if (img.closest('.se-module-oglink, .se-oglink, .se-section-oglink, [data-linkdata]:not([data-linktype="img"])')) {
            return false;
        }

        if (img.closest('.area_profile, .blog_profile, #blog-profile, .comment_area, .wrap_post_btn, .post_tag, .post-meta, .post_writer, .cbox_module')) {
            return false;
        }

        const src = (img.currentSrc || img.src || img.getAttribute('data-lazy-src') || '').toLowerCase();
        if (!src) return false;
        if (src.startsWith('data:') || src.startsWith('blob:')) return false;
        if (src.includes('profile') || src.includes('spacer') || src.includes('/spc.gif')) return false;

        const width = Number(img.getAttribute('data-width')) || img.naturalWidth || img.width || 0;
        const height = Number(img.getAttribute('data-height')) || img.naturalHeight || img.height || 0;

        if (width > 0 && height > 0 && (width < 120 || height < 120)) {
            return false;
        }

        return true;
    }

    function hasNaverImageLink(img) {
        try {
            return !!img.closest('.se-component.se-image, .se-module-image')
                ?.querySelector('a[data-linktype="img"][data-linkdata]');
        } catch (_) {
            return false;
        }
    }

    function pushImage(images, processedUrls, candidate) {
        if (!candidate || !candidate.src) return;

        const key = candidate.src;
        if (processedUrls.has(key)) return;
        processedUrls.add(key);

        images.push({
            idx: images.length,
            src: candidate.src,
            alt: candidate.alt || 'image_' + images.length,
            width: candidate.width || 0,
            height: candidate.height || 0
        });
    }

    const images = [];
    const processedUrls = new Set();
    const documents = getAccessibleDocuments(document);

    documents.forEach((doc) => {
        const roots = getContentRoots(doc);
        roots.forEach((root) => {
            const imageLinks = root.querySelectorAll('a[data-linktype="img"][data-linkdata]');
            imageLinks.forEach((link) => {
                try {
                    const linkData = parseLinkData(link.getAttribute('data-linkdata'));
                    if (!linkData || !linkData.src) return;

                    const imageUrl = normalizeImageUrl(linkData.src, doc);
                    if (!imageUrl) return;

                    pushImage(images, processedUrls, {
                        src: imageUrl,
                        alt: link.getAttribute('title') || '',
                        width: Number(linkData.originalWidth) || 0,
                        height: Number(linkData.originalHeight) || 0
                    });
                } catch (_) {}
            });
        });
    });

    documents.forEach((doc) => {
        const roots = getContentRoots(doc);
        roots.forEach((root) => {
            const allImages = root.querySelectorAll('img');
            allImages.forEach((img) => {
                try {
                    if (!isMeaningfulContentImage(img, root)) return;
                    if (hasNaverImageLink(img)) return;

                    const rawSrc =
                        img.getAttribute('data-original-url') ||
                        img.getAttribute('data-lazy-src') ||
                        img.getAttribute('data-src') ||
                        img.getAttribute('data-original') ||
                        img.currentSrc ||
                        img.src ||
                        '';

                    const imageUrl = normalizeImageUrl(rawSrc, doc);
                    if (!imageUrl) return;

                    pushImage(images, processedUrls, {
                        src: imageUrl,
                        alt: img.alt || img.getAttribute('title') || '',
                        width: Number(img.getAttribute('data-width')) || img.naturalWidth || img.width || 0,
                        height: Number(img.getAttribute('data-height')) || img.naturalHeight || img.height || 0
                    });
                } catch (_) {}
            });
        });
    });
    
    return JSON.stringify(images);
})();
""";
        }
    }
}

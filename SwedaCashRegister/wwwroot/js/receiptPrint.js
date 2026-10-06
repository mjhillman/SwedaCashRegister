window.printReceipt = (elementId) => {
    const el = document.getElementById(elementId);
    if (!el) {
        console.error(`printReceipt: element #${elementId} not found`);
        return;
    }

    // Use textContent if you want plain text (safer for receipts).
    // Use innerHTML only if you trust the source and want formatting.
    const printContents = el.textContent;

    const printWindow = window.open('', '_blank', 'width=400,height=600');
    if (!printWindow) {
        alert('Please allow popups to print the receipt.');
        return;
    }

    const html = `
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <title>Receipt</title>
            <style>
                @page { margin: 0; }
                body {
                    font-family: 'Courier New', monospace;
                    font-size: 12px;
                    margin: 0;
                    padding: 10px;
                    width: 80mm; /* typical thermal receipt width */
                }
                pre { white-space: pre-wrap; margin: 0; word-wrap: break-word; }
            </style>
        </head>
        <body>
            <pre>${escapeHtml(printContents)}</pre>
        </body>
        </html>
    `;

    printWindow.document.open();
    printWindow.document.write(html);
    printWindow.document.close();

    // Wait for content to render before printing
    const doPrint = () => {
        printWindow.focus();
        printWindow.print();
    };

    // 'load' fires after resources are ready
    if (printWindow.document.readyState === 'complete') {
        doPrint();
    } else {
        printWindow.onload = doPrint;
    }

    // Close after the print dialog is dismissed
    printWindow.onafterprint = () => printWindow.close();
};

// Minimal HTML escaper to prevent breaking out of <pre> and XSS
function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}
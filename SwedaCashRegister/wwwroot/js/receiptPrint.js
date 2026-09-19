
window.printReceipt = (elementId) => {
    const printContents = document.getElementById(elementId).innerHTML;
    const printWindow = window.open('', '_blank', 'width=400,height=600');

    printWindow.document.write(`
        <html>
        <head>
            <title>Receipt</title>
            <style>
                body { font-family: 'Courier New', monospace; font-size: 12px; margin: 0; padding: 10px; }
                pre { white-space: pre-wrap; margin: 0; }
            </style>
        </head>
        <body>
            <pre>${printContents}</pre>
        </body>
        </html>
    `);

    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
    printWindow.close();
};
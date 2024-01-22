window.downloadFile = (fileName, file) => {
    const link = document.createElement('a');
    link.href = file;
    link.download = fileName ?? '';
    link.click();
    link.remove();
}
function SaveXls(fileName, byteBase64) {
    const link = document.createElement('a');
    link.download = fileName ?? '';
    link.href = 'data:application/octet-stream;base64,' + byteBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
function scrollToEnd(ref) {
    ref.scrollTop = ref.scrollHeight;
}
window.trapFocus = (firstElement, lastElement) => {
    const focusableElements = Array.from(
        document.querySelectorAll("button, [href], input, select, textarea, [tabindex]:not([tabindex='-1'])")
    );

    const firstFocusableIndex = focusableElements.indexOf(firstElement);
    const lastFocusableIndex = focusableElements.indexOf(lastElement);

    document.addEventListener("keydown", trapFocusHandler);

    function trapFocusHandler(event) {
        if (event.key === "Tab" || event.keyCode === 9) {
            if (event.shiftKey && document.activeElement === firstElement) {
                event.preventDefault();
                focusableElements[lastFocusableIndex].focus();
            } else if (!event.shiftKey && document.activeElement === focusableElements[lastFocusableIndex]) {
                event.preventDefault();
                focusableElements[firstFocusableIndex].focus();
            }
        }
    }
};
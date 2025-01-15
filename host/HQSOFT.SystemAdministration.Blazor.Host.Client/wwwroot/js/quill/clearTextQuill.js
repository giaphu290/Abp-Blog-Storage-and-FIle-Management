window.clearTextQuill = {
    clearContent: function (editorId) {
        var quill = new Quill('#' + editorId);
        quill.setText('');
    }
};
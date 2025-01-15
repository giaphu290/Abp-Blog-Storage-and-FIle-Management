window.QuillFunctions = {
    createQuill: function (
        quillElement, toolBar, userList, readOnly,
        placeholder, theme, debugLevel) {

        var options = {
            debug: debugLevel,
            modules: {
                imageResize: {
                    displaySize: true
                },
                toolbar: toolBar,
                mention: {
                    allowedChars: /^[A-Za-z\sÅÄÖåäö]*$/,
                    mentionDenotationChars: ["@", "#"],
                    linkTarget: '_self',
                    // Your mention configuration
                    source: function (searchTerm, renderList, mentionChar) {
                        if (searchTerm.length === 0) {
                            renderList(userList, searchTerm);
                        } else {
                            var matches = [];
                            for (var i = 0; i < userList.length; i++) {
                                if (~userList[i].value.toLowerCase().indexOf(searchTerm.toLowerCase())) {
                                    matches.push(userList[i]);
                                }
                            }
                            renderList(matches, searchTerm);
                        }
                    }
                }
            },
            placeholder: placeholder,
            readOnly: readOnly,
            theme: theme
        };

        new Quill(quillElement, options);
    },
    getMentionedUsers: function (quillElement) {
        var delta = quillElement.__quill.getContents();
        var ops = delta.ops;
        var mentionedUsers = [];

        for (var i = 0; i < ops.length; i++) {
            var op = ops[i];
            if (op.insert && op.insert.mention) {
                mentionedUsers.push(op.insert.mention.id);
            }
        }

        return mentionedUsers;
    },
    getQuillContent: function (quillElement) {
        return JSON.stringify(quillElement.__quill.getContents());
    },
    getQuillText: function (quillElement) {
        return quillElement.__quill.getText();
    },
    getQuillHTML: function (quillElement) {
        return quillElement.__quill.root.innerHTML;
    },
    clearContent: function (quillElement) {
        var quill = quillElement.__quill;
        quill.setText('');
    },
    loadQuillContent: function (quillElement, quillContent) {
        content = JSON.parse(quillContent);
        return quillElement.__quill.setContents(content, 'api');
    },
    enableQuillEditor: function (quillElement, mode) {
        quillElement.__quill.enable(mode);
    }
};
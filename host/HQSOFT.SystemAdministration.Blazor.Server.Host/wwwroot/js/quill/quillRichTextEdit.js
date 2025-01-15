
var mentionedUserIds = [];
window.QuillFunctions = {
    createQuill: function (quillElement, userList) {
        var options = {
            debug: 'info',
            modules: {
                toolbar: '#toolbar', 
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
                    },
                    onSelect: function (item, insertItem) {
                        var mentionId = item.id; // Lấy ID của người dùng đang được chọn 
                        mentionedUserIds.push(mentionId); // Thêm ID vào danh sách
                        insertItem(item);

                        // Gửi danh sách ID của người dùng đến máy chủ
                        dotnetHelper.invokeMethodAsync('HandleMentionIds', mentionedUserIds);
                    }
                }
            },
            placeholder: 'Start typing @ to mention ...',
            readOnly: false,
            theme: 'snow'
        };
        // set quill at the object we can call
        // methods on later
        new Quill(quillElement, options);
    },
    getQuillContent: function (quillControl) {
        return JSON.stringify(quillControl.__quill.getContents());
    },
    getQuillText: function (quillControl) {
        return quillControl.__quill.getText();
    },
    getQuillHTML: function (quillControl) {
        if (quillControl.__quill.root) {
            return quillControl.__quill.root.innerHTML;
        }
        return ''; // or handle the case when the root is null
    },
    loadQuillContent: function (quillControl, quillContent) {
        content = JSON.parse(quillContent);
        return quillControl.__quill.setContents(content, 'api');
    }
};
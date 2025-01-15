var mentionedUserIds = [];
window.quillInterop = {
    initializeQuillEditor: function (elementId, userList, dotnetHelper) {
        var toolbarOptions = [ 
            //[{ 'header': [1, 2, 3, 4, 5, 6, false] }],
            //// dropdown with defaults from theme
            //[{ 'font': [] }], 
            ['bold', 'italic', 'underline', 'strike'],        // toggled buttons 
            [{ 'color': [] }, { 'background': [] }],
            ['blockquote', 'code-block'],
            [{ 'direction': 'rtl' }],
            ['link', 'image'],
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            [{ 'align': [] }],         // text direction 
            [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
            [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
            ['clean'],                                         // remove formatting button         
        ]; 
        var quill = new Quill('#' + elementId, {
            modules: {
                imageResize: {
                    displaySize: true
                },
                toolbar: {
                    container: toolbarOptions
                },
                clipboard: {
                    matchers: [
                        ["img", customImgForPaste]
                    ],
                },
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
            theme: 'snow'
        });

        function customImgForPaste() {
            let fileInput = this.container.querySelector('input.ql-image[type=file]');
            if (fileInput == null) {
                fileInput = document.createElement('input');
                fileInput.setAttribute('type', 'file');
                fileInput.setAttribute('accept', 'image/png, image/gif, image/jpeg, image/bmp, image/x-icon');
                fileInput.classList.add('ql-image');
                fileInput.addEventListener('change', () => {
                    if (fileInput.files != null && fileInput.files[0] != null) {
                        let reader = new FileReader();
                        reader.onload = (e) => {
                            let range = this.quill.getSelection(true);
                            this.quill.updateContents(new Delta()
                                .retain(range.index)
                                .delete(range.length)
                                .insert({ image: e.target.result })
                                , Emitter.sources.USER);
                            fileInput.value = "";
                        }
                        reader.readAsDataURL(fileInput.files[0]);
                    }
                });
                this.container.appendChild(fileInput);
            }
            fileInput.click();
        }

        quill.on('text-change', function () {
            updateOutput();
        });

        function getQuillHtml() {
            return quill.root.innerHTML;
        }

        function getQuillText() {
            return quill.getText();
        }

        function getQuillContent() {
            return JSON.stringify(quill.getContents());
        }

        function updateOutput() {
            let html = getQuillHtml();
            let text = getQuillText();
            let content = getQuillContent();
            dotnetHelper.invokeMethodAsync('UpdateQuillOutput', html, text, content);
        }

        updateOutput();

        var quillData = {
            options: {
                modules: quill.getModule('toolbar').options,
                placeholder: quill.root.dataset.placeholder,
                theme: quill.theme.constructor.name
            }
        };
        var quillDataJson = JSON.stringify(quillData);
        return quillDataJson;
    },  
    //Clear data
    clearQuillContent: function () {
        var quill = document.querySelector('#editor .ql-editor');
        quill.innerHTML = ""; // Clear the HTML content of the Quill editor
        quill.innerText = ""; // Clear the plain text content of the Quill editor 
        mentionedUserIds = [];
    }
};

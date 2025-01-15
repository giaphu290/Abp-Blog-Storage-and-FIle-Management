
window.AssignGotFocus = function () {
    var _input;
    [...document.querySelectorAll('.focus-value')].forEach(__input => {
        __input.addEventListener("focus", function (event) {
            // Sử dụng setTimeout để đảm bảo hành động chọn văn bản diễn ra sau khi phần tử đã lấy được tiêu điểm.
            setTimeout(function () { __input.select(); }, 0);
        });
    });
};



// JavaScript Function to Capture Key Events
window.addHotkeyListener = (element, dotNetHelper) => {
    const handler = (e) => {
        // Kiểm tra nếu người dùng nhấn tổ hợp phím Ctrl + S, B, hoặc I (không phân biệt chữ hoa/thường).
        if ((e.ctrlKey && (e.key === 's' || e.key === 'S' || e.key === 'b' || e.key === 'B' || e.key === 'i' || e.key === 'I'))) {
            e.preventDefault(); // Ngăn chặn hành động mặc định của trình duyệt cho các tổ hợp phím này.
            // Gọi phương thức 'HandleHotkey' từ phía .NET và truyền thông tin về phím được nhấn.
            dotNetHelper.invokeMethodAsync('HandleHotkey', {
                ctrlKey: e.ctrlKey,
                key: e.key
            });
        }
    };
    // Gán hàm xử lý sự kiện hotkey vào thuộc tính hotkeyHandler của element.
    element.hotkeyHandler = handler;
    // Thêm event listener để lắng nghe sự kiện 'keydown' trên tài liệu.
    document.addEventListener('keydown', handler);
};

// Hàm này xóa bỏ hotkey listener đã được thêm vào bởi hàm addHotkeyListener.
window.removeHotkeyListener = (element) => {
    if (element.hotkeyHandler) {
        // Gỡ bỏ event listener cho sự kiện 'keydown' trên tài liệu.
        document.removeEventListener('keydown', element.hotkeyHandler);
        element.hotkeyHandler = null; // Đặt thuộc tính hotkeyHandler về null.
    }
};

//check component is focused
window.focusHandler = {
    isElementFocused: function (elementId) {
        var element = document.getElementById(elementId);
        // Trả về true nếu phần tử tồn tại và là phần tử hiện đang có tiêu điểm.
        return element && document.activeElement === element;
    }
};



// Hàm này mở một URL trong một tab mới của trình duyệt.
window.openInNewTab = function (url) {
    var newTab = window.open(url, '_blank'); // Mở URL trong một tab mới.
    newTab.focus(); // Đưa tab mới vào tiêu điểm.
};




// Hàm này ẩn sidebar, toolbar và footer trên giao diện để kích hoạt chế độ toàn màn hình.
function FullScreen() {

    var lpxSidebarContainer = document.querySelector('.lpx-sidebar-container');
    var lpxfootbarcontainer = document.querySelector('.lpx-footbar-container');

    if (lpxfootbarcontainer !== null) {
        lpxfootbarcontainer.style.display = 'none';
        document.querySelector('.lpx-footbar-container').style.display = 'none';
        document.querySelector('body .lpx-footbar-container').style.margin = '0';
    }

    if (lpxSidebarContainer !== null) {
        lpxSidebarContainer.style.display = 'none';
        document.querySelector('.lpx-toolbar-container').style.display = 'none';
        document.querySelector('body .lpx-content-container').style.margin = '0';
        document.querySelector('.lpx-topbar-container').style.display = 'block';
    }

    else {
        document.getElementsByClassName('lp-sidebar')[0].hidden = true;
        document.querySelector('body .lp-content').style.padding = '0 36px 36px 24px';
    }
};


// Hàm này khôi phục lại giao diện về trạng thái bình thường từ chế độ toàn màn hình.
function UnFullScreen() {
    var navbarBrand = document.getElementsByClassName('navbar-brand')[0];

    // Nếu navbar tồn tại, hiển thị lại sidebar và điều chỉnh padding.
    if (navbarBrand !== undefined) {
        document.getElementsByClassName('lp-sidebar')[0].hidden = false;
        document.querySelector('body .lp-content').style.padding = '';
    } else {
        // Nếu không, hiển thị lại sidebar, toolbar và content container.
        var lpSidebar = document.getElementsByClassName('lpx-sidebar-container')[0];
        var lpToolbar = document.getElementsByClassName('lpx-toolbar-container')[0];
        var lpContent = document.querySelector('.lpx-content-container');

        lpSidebar.style.display = 'block';
        lpToolbar.style.display = 'block';
        document.querySelector('body .lpx-content-container').style.margin = '';

        var lpxfootbarcontainer = document.querySelector('.lpx-footbar-container');
        var lpxfootbar = document.querySelector('.lpx-footbar');

        // Hiển thị lại footbar nếu nó tồn tại.
        if (lpxfootbarcontainer != null) {
            lpxfootbarcontainer.style.display = 'block';
            document.querySelector('body .lpx-footbar-container').style.margin = '';
        }
        if (lpxfootbar != null) {
            lpxfootbar.style.display = 'block';
            document.querySelector('body .lpx-footbar').style.margin = '';
        }
    }
};









//HIỂN THỊ TOOLBAR BỊ MẤT Ở MÀN HÌNH KHI NHÚNG VÀO WEB

// HANGFIRE DASHBOARD
function registerUtilsBlazorMethod(blazorObject) {
    window.blazorObject = blazorObject;
}

function invokeUtilsBlazorPopup() {
    if (window.blazorObject) {
        window.blazorObject.invokeMethodAsync('ResetToolbarItemsAsync');
    }
}

function onIframeLoad() {
    customizeHangfireDashboard();
}

function customizeHangfireDashboard() {
    var iframe = document.getElementById('hangfire-iframe');
    iframe.onload = function () {
        var doc = iframe.contentDocument || iframe.contentWindow.document;
        var style = doc.createElement('style');
        style.innerHTML = `  
                .navbar-right {
                    display:none;
                } 
        `;
        doc.head.appendChild(style);
        invokeUtilsBlazorPopup();
    };
}


// ELSA DASHBOARD

function registerElsaUtilsBlazorMethod(blazorObject1) {
    window.blazorObject1 = blazorObject1;
}

function invokeElsaUtilsBlazorPopup() {
    if (window.blazorObject1) {
        window.blazorObject1.invokeMethodAsync('ResetElsaToolbarItemsAsync');
    }
}

function onElsaLoaded() {
    invokeElsaUtilsBlazorPopup();
}

// Cờ để kiểm tra xem các script đã được tải chưa
window.isElsaScriptsLoaded = false;

// Xóa script và stylesheet khi người dùng rời khỏi trang
window.addEventListener('beforeunload', function () {
    // Xóa các script và stylesheet liên quan đến Elsa
    const scripts = document.querySelectorAll('script[src*="elsa-workflows-studio"]');
    scripts.forEach(script => script.remove());

    const links = document.querySelectorAll('link[href*="elsa-workflows-studio"]');
    links.forEach(link => link.remove());
});

// Tải các script và stylesheet
function loadElsaScripts() {
    if (window.isElsaScriptsLoaded) return;
    window.isElsaScriptsLoaded = true;

    // Kiểm tra xem script đã được thêm chưa
    if (document.querySelector('script[src="/_content/Elsa.Designer.Components.Web/monaco-editor/min/vs/loader.js"]')) return;
    if (document.querySelector('script[src="/_content/Elsa.Designer.Components.Web/elsa-workflows-studio/elsa-workflows-studio.esm.js"]')) return;
}

// Gọi hàm tải các script sau khi trang đã được tải
document.addEventListener("DOMContentLoaded", loadElsaScripts);

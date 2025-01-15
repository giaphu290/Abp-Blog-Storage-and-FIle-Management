let isFrontCamera = true; // Biến lưu trạng thái camera (trước/sau)

function ready() {
    if (document.readyState == 'complete') {
        Webcam.set({
            width: 320,
            height: 240,
            image_format: 'jpeg',
            jpeg_quality: 90,
            facingMode: isFrontCamera ? 'user' : 'environment'
        });
        try {
            Webcam.attach('#camera');
        } catch (e) {
            alert(e);
        }
    }
}

function stopCamera() {
    Webcam.reset();
}

async function take_snapshot() {
    return new Promise((resolve, reject) => {
        Webcam.snap(function (data_uri) {
            resolve(data_uri);
        });
    });
}

function switchCamera() {
    stopCamera(); // Dừng camera hiện tại

    isFrontCamera = !isFrontCamera; // Chuyển đổi giá trị của biến isFrontCamera

    // Cấu hình Webcam với camera mới
    Webcam.set({
        width: 320,
        height: 240,
        image_format: 'jpeg',
        jpeg_quality: 90,
        facingMode: isFrontCamera ? 'user' : 'environment' // 'user' cho camera trước, 'environment' cho camera sau
    });

    try {
        Webcam.attach('#camera'); // Kết nối với camera mới
    } catch (e) {
        alert(e);
    }
}

navigator.mediaDevices.enumerateDevices()
    .then(function (devices) {
        devices.forEach(function (device) {
            console.log('Device ID: ', device.deviceId);
            console.log('Kind: ', device.kind);
            console.log('Label: ', device.label);
            console.log('------------------------');
        });
    })
    .catch(function (err) {
        console.error('Error: ', err);
    });
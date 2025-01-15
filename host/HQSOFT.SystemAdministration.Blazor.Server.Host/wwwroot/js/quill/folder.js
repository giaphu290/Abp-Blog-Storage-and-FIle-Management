async function selectFolder() {
    const folderHandle = await window.showDirectoryPicker();
    console.log(`Selected folder: ${folderHandle.name}`);
    return folderHandle.name; // Trả về tên folder
}
//window.downloadFile = (url, fileName) => {
//    const a = document.createElement("a");
//    a.href = url;
//    a.download = fileName || "contract";
//    document.body.appendChild(a);
//    a.click();
//    document.body.removeChild(a);
//};

//window.blazorDownloadContract = (dataUrl, fileName) => {
//    const a = document.createElement('a');
//    a.href = dataUrl;
//    a.download = fileName;
//    document.body.appendChild(a);
//    a.click();
//    document.body.removeChild(a);
//};

window.bootstrapModal = {
    show: function (id) {
        var modal = new bootstrap.Modal(document.querySelector(id));
        modal.show();
    }
};

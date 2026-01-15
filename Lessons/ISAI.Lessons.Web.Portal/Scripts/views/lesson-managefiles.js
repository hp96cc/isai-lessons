var hostUrl = '/Lesson/';

ej.filemanager.FileManager.Inject(ej.filemanager.DetailsView, ej.filemanager.Toolbar, ej.filemanager.NavigationPane);

var filemanagerInstance = new ej.filemanager.FileManager({
    ajaxSettings: {
        url: hostUrl + 'FileOperations?lessonId=' + lessonId,
        getImageUrl: hostUrl + 'GetImage?lessonId=' + lessonId,
        uploadUrl: hostUrl + 'Upload?lessonId=' + lessonId,
        downloadUrl: hostUrl + 'Download?lessonId=' + lessonId
    },
    toolbarSettings: {
       items: [
        "Upload",
        "SortBy",
        "Refresh",
        "Selection",
        "View",
        /*"Cut",
        "Copy",
        "Delete",*/
        "Details",
        "Download",
       ]

    },

    contextMenuSettings: {
        file: ["Open", "Download", "Delete", "Rename"],
        folder: ["Open", "Delete", "Rename"],
        layout: ["SortBy", "Refresh"]
    },


    view: "Details"
});

filemanagerInstance.appendTo('#filemanager');

$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const ckEditorFuncNum = urlParams.get("CKEditorFuncNum");
    $(function () {
        $("img").click(function () {
            const fileurl = "/uploads/" + $(this).attr("title");
            window.opener.CKEDITOR.tools.callFunction(ckEditorFuncNum, fileurl);
            window.close();
        }).hover(function() {
            $(this).css("cursor", "pointer")
        });
    });
});

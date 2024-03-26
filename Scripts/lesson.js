document.addEventListener("DOMContentLoaded", () => {
    var header = document.getElementById("sticky-header-lesson");

    var body = document.getElementsByTagName("body");
    body[0].style.overflowY = "Hidden";

    var divTop = $(".to-top");
    divTop.css("margin-top", header.offsetHeight + "px");

    var btnAddNote = document.querySelector(".btn-add-note");
    btnAddNote.addEventListener("click", () => {
        let video = document.getElementById("video_lesson");
        var noteTime = $(".note-time");
        noteTime.text(convertTime(video.currentTime));

        setTimeout(() => {
            var modalBackdrop = document.querySelector(".modal-backdrop");
            modalBackdrop.setAttribute("style", "opacity: 0 !important;");
        }, 1);
    });

    //ajax
    $("#btn_submit_add_note").click(function () {
        // Thêm giá trị second vào FormData
        var video = document.getElementById("video_lesson");
        var formData = new FormData($("#form_add_note")[0]);
        let time = Math.floor(video.currentTime);
        formData.append("time", time);
        formData.append("lessonId", 1);

        if ($("#inp_note").val() != "") {
            $.ajax({
                url: $("#form_add_note").attr('action'),
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                headers: {
                    RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val(),
                },
                success: function (result) {
                    $("#inp_note").val("");
                    toastr.success(result.message, "Notify");
                },
                error: function (xhr, status, error) {
                    console.log(error);
                    toastr.error(error, "Error");
                },
            });
        } else {
            toastr.error("Không được để trống nội dung", "Error");
        }
    });
});

function convertTime(decimalTime) {
    var hours = Math.floor(decimalTime / 3600);
    var remainingSeconds = decimalTime % 3600;
    var minutes = Math.floor(remainingSeconds / 60);
    var seconds = Math.floor(remainingSeconds % 60);

    var result =
        (hours < 10 ? "0" + hours : hours) +
        ":" +
        (minutes < 10 ? "0" + minutes : minutes);

    if (seconds < 10) {
        result += ":0" + seconds;
    } else {
        result += ":" + seconds;
    }

    return result;
}

function setTimeVideo(second) {
    let video = document.getElementById("video_lesson");
    video.currentTime = second;
}

window.addEventListener("scroll", function () {
    var header = document.getElementById("sticky-header");
    var scrollTop =
        window.pageYOffset ||
        document.documentElement.scrollTop ||
        document.body.scrollTop ||
        0;

    if (scrollTop > 50) {
        header.style.position = "fixed";
    } else {
        header.style.position = "relative";
    }
});

function truncateString(input, maxLength) {
    if (input.length <= maxLength) {
        return input;
    } else {
        return input.substring(0, maxLength) + "...";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    var lessonTitle = truncateString($('.menu-footer .title-lesson').text(), 27);
    $('.menu-footer .title-lesson').text(lessonTitle);

    var header = $('.sticky-header-lesson');
    var footer = document.getElementById("footer_lesson");

    let heightHeader = header[0].offsetHeight !== 0 ? header[0].offsetHeight : header[1].offsetHeight;

    var divTop = $(".to-top");
    divTop.css("margin-top", heightHeader + "px");

    var divBottom = $(".to-bottom");
    divTop.css("margin-bottom", footer.offsetHeight + "px");

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


    var urlParams = new URLSearchParams(window.location.search);
    var lessonId = urlParams.get('lessonId');

    //note
    $("#btn_submit_add_note").click(function () {
        var video = document.getElementById("video_lesson");
        var formData = new FormData($("#form_add_note")[0]);
        let time = Math.floor(video.currentTime);
        formData.append("time", time);
        formData.append("lessonId", lessonId);

        if ($("#inp_note").val() != "") {
            $.ajax({
                url: $("#form_add_note").attr('action'),
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                headers: {
                    RequestVerificationToken: $('#form_add_note input:hidden[name="__RequestVerificationToken"]').val(),
                },
                success: function (result) {
                    $("#inp_note").val("");
                    toastr.success(result.message, "Notify");
                },
                error: function (xhr) {
                    toastr.error(xhr.responseJSON.message, "Error");
                },
            });
        } else {
            toastr.error("Không được để trống nội dung", "Error");
        }
    });

    $(".btn_open_notes").click(() => {
        $.ajax({
            url: $('.btn_open_notes').data("ajax") + "?lessonId=" + lessonId,
            type: "GET",
            processData: false,
            contentType: false,
            success: function (result) {
                $("#body_notes").html(result.data);

                //Delete note
                $('.btn-delete-note').click((e) => {
                    let eTmp = e.target;
                    let divParent = eTmp.closest(".note-item");

                    let idNote = divParent.getAttribute("data-note");
                    let url = divParent.getAttribute("data-ajax-delete");
                    $.ajax({
                        url: url + "/" + idNote,
                        type: "POST",
                        headers: {
                            "X-HTTP-Method-Override": "DELETE" // Gửi header để chỉ định yêu cầu là DELETE
                        },
                        success: function (result) {
                            divParent.remove();
                        },
                        error: function (xhr, status, error) {
                            toastr.error("Error Unknow, Please try again", "Error");
                        }
                    });
                })

                //Edit Note

                $('.btn-edit-note').click((e) => {
                    let eTmp = e.target;
                    let divParent = eTmp.closest(".note-item");
                    let idNote = divParent.getAttribute("data-note");

                    let eTextContent = divParent.querySelector(".note-item-content");
                    eTextContent.classList.add("hidden");

                    let form = divParent.querySelector(".form");
                    let input = form.querySelector(".input-content-note");
                    input.value = eTextContent.textContent.trim();
                    input.classList.remove("hidden");
                    let danger = form.querySelector(".text-danger");
                    danger.classList.remove("hidden");
                    let csrf = form.querySelector('input[type="hidden"][name="__RequestVerificationToken"]').value;

                    input.addEventListener("keypress", (e) => {
                        if (e.keyCode === 13) {
                            let newText = e.target.value.trim();

                            if (newText !== '') {
                                let formData = new FormData(form);
                                formData.append("id", idNote);
                                $.ajax({
                                    url: form.getAttribute('action'),
                                    type: "POST",
                                    data: formData,
                                    processData: false,
                                    contentType: false,
                                    headers: {
                                        RequestVerificationToken: csrf,
                                    },
                                    success: function (result) {

                                        eTextContent.textContent = newText;
                                        e.target.classList.add("hidden");
                                        danger.classList.add("hidden");

                                        eTextContent.classList.remove("hidden");

                                        toastr.success(result.message, "Notify");
                                    },
                                    error: function (xhr, status, error) {
                                        toastr.error("Error Unknown, Please Try Again", "Error");
                                    },
                                });
                            }
                        }
                    });
                })
            },
            error: function (xhr, status, error) {
                toastr.error("Error Unknow, Please try again", "Error");
            },
        });
    })

    //comment
    $(".btn-comment").click((e) => {
        $.ajax({
            url: $('.btn-comment').data("ajax") + "?lessonId=" + lessonId,
            type: "GET",
            processData: false,
            contentType: false,
            success: function (result) {
                $("#body_comments").html(result.data);

                $('.btn-delete-comment').click((e) => {
                    let eTmp = e.target;
                    let divParent = eTmp.closest(".comment-item");

                    let idComment = divParent.getAttribute("data-comment");
                    let url = divParent.getAttribute("data-ajax-delete");
                    $.ajax({
                        url: url + "/" + idComment,
                        type: "POST",
                        headers: {
                            "X-HTTP-Method-Override": "DELETE"
                        },
                        success: function (result) {
                            divParent.remove();
                        },
                        error: function (xhr, status, error) {
                            toastr.error("Error Unknow, Please try again", "Error");
                        }
                    });
                })
            },
            error: function (xhr, status, error) {
                toastr.error("Error Unknow, Please try again", "Error");
            },
        });
    })

    $(".btn-save-comment").click(() => {
        let content = $("#text_comment").val().trim();
        if (content != "") {
            var formData = new FormData($("#form_add_comment")[0]);
            formData.append("lessonId", lessonId);

            $.ajax({
                url: $("#form_add_comment").attr('action'),
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                headers: {
                    RequestVerificationToken: $('#form_add_comment input:hidden[name="__RequestVerificationToken"]').val(),
                },
                success: function (result) {
                    console.log(result.data);
                    if (result.data.type == 1) {
                        let classCommentIcon = result.data.role == 2 ? "fa-user-shield" : "fa-user";
                        let newCommentHtml = `
                        <div class="mb-3 d-flex comment-item justify-space-between" data-comment="${result.data.id}" data-ajax-delete="${result.data.ajax_delete}">
                            <div>
                                <label for="text_comment" class="form-label me-3">
                                    <i class="fas ${classCommentIcon} fs-5 text-primary" style="margin-top: 0.2rem !important" aria-hidden="true"></i>
                                </label>
                                <div>
                                    <h5 class="text-dark">${result.data.name}</h5>
                                    <p class="text-dark" style="font-family: PoppinsVH">${result.data.content}</p>
                                </div>
                            </div>
                            <button class="btn-none text-gray btn-delete-comment d-flex">
                                <i class="fa-solid fa-trash me-1 note-item-icon"></i>
                            </button>
                        </div>`;

                        $("#body_comments").prepend(newCommentHtml);
                    }
                    $("#text_comment").val("");
                    toastr.success(result.message, "Notify");


                    $('.btn-delete-comment').click((e) => {
                        let eTmp = e.target;
                        let divParent = eTmp.closest(".comment-item");

                        let idComment = divParent.getAttribute("data-comment");
                        let url = divParent.getAttribute("data-ajax-delete");
                        $.ajax({
                            url: url + "/" + idComment,
                            type: "POST",
                            headers: {
                                "X-HTTP-Method-Override": "DELETE"
                            },
                            success: function (result) {
                                divParent.remove();
                            },
                            error: function (xhr, status, error) {
                                toastr.error("Error Unknow, Please try again", "Error");
                            }
                        });
                    })
                },
                error: function (xhr) {
                    toastr.error(xhr.responseJSON.message, "Error");
                },
            });
        } else {
            toastr.error("Không được để trống nội dung", "Error");
        }
    })
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

//Video
document.addEventListener("DOMContentLoaded", () => {
    var lastLink = $('footer').find('a.last')[0];
    if (lastLink != null) {
        lastLink.addEventListener("click", (e) => {
            if (e.target.classList.contains("disable")) {
                e.preventDefault();
            }
        })
    }

    var video = document.getElementById('video_lesson');
    let flag = false;
    var timeOnPage = 0;
    countDownTimer = setInterval(function () {
        timeOnPage++;
        if (timeOnPage >= video.duration * 2 / 3) {
            clearInterval(countDownTimer);
            flag = true;
        }
    }, 1000);
    video.addEventListener('timeupdate', function () {
        var currentTime = video.currentTime;
        var duration = video.duration;
        var progress = currentTime / duration;
        if (progress >= (2 / 3) && flag == true) {
            var urlParams = new URLSearchParams(window.location.search);
            var lessonId = urlParams.get('lessonId');
            var courseId = urlParams.get('courseId');

            let formData = new FormData($("#form_video_lesson")[0]);
            formData.append("courseId", courseId);
            formData.append("lessonId", lessonId);

            var lastLink = $('footer').find('a.last')[0];
            if (lastLink.classList.contains("disable")) {
                $.ajax({
                    url: $("#form_video_lesson").attr('action'),
                    type: "POST",
                    data: formData,
                    processData: false,
                    contentType: false,
                    headers: {
                        RequestVerificationToken: $('#form_video_lesson input:hidden[name="__RequestVerificationToken"]').val(),
                    },
                    success: function (result) {
                        var element = $('.div-lesson.active');
                        var nextElement = element.next()[0];
                        nextElement.classList.remove('disable');

                        var lastLink = $('footer').find('a.last')[0];
                        lastLink.classList.remove("disable");
                    },
                });
            }
        }
    });
})

//lesson

document.addEventListener("DOMContentLoaded", () => {
    const windowWidth = window.innerWidth;
    $(".open-lesson").click(() => {
        if (windowWidth < 992) {
            let modalLesson = new bootstrap.Modal(document.getElementById('modal_lesson'))
            modalLesson.show()
        }
    })
})

// exercise
document.addEventListener("DOMContentLoaded", () => {
    $(".btn-save-down-exercise").click(() => {
        var urlParams = new URLSearchParams(window.location.search);
        var lessonId = urlParams.get('lessonId');

        $.ajax({
            url: "/Exercise/GetExercise?lessonId=" + lessonId,
            type: "GET",
            processData: false,
            contentType: false,
            success: function (result) {
                var a = document.createElement('a');
                var url = result.data;
                a.href = url;
                a.download = 'file.docx';
                a.target = '_blank';
                document.body.append(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
            },
            error: function (xhr) {
                toastr.error(xhr.responseJSON.message, "Error");
            },
        });
    })
})
// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.querySelectorAll(".toggleCommentForm").forEach(button => {

    button.addEventListener('click', function () {

        var postId = this.getAttribute('data-post-id');
        var form = document.querySelector('.commentForm[data-post-id="' + postId + '"]');
        if (form.style.display === "none") {
            form.style.display = "block";
            this.textContent = "Cancelar"

            this.classList.remove("btn-primary")

            this.classList.add("btn-danger")

        } else {
            form.style.display = "none";
            this.textContent = "Comentar"
            this.classList.remove("btn-danger")

            this.classList.add("btn-primary")


        }

    });
})


document.querySelectorAll('.toggleReplyForm').forEach(button => {
    button.addEventListener('click', function () {
        var commentId = this.getAttribute('data-comment-id');
        var form = document.querySelector('.replyForm[data-comment-id="' + commentId + '"]');
        if (form.style.display === "none") {
            form.style.display = "block";
            this.textContent = "Cancelar";
        } else {
            form.style.display = "none";
            this.textContent = "Responder";
        }
    });
});

const toggleInputs = () => {
    var textOption = document.getElementById('textOption').checked;
    var imageOption = document.getElementById('imageOption').checked;
    var videoOption = document.getElementById('videoOption').checked;

    var imageInput = document.getElementById('imageInput');
    var videoInput = document.getElementById('videoInput');

    if (textOption) {
        imageInput.style.display = 'none';
        videoInput.style.display = 'none';
    } else if (imageOption) {
        imageInput.style.display = 'block';
        videoInput.style.display = 'none';
    } else if (videoOption) {
        imageInput.style.display = 'none';
        videoInput.style.display = 'block';
    }
}
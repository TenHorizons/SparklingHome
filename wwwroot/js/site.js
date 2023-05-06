// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    $('[data-toggle="tooltip"]').tooltip()
})

$(function () {
    setInterval(function () {
        $.get('/Home/GetNewMessages', function (data) {
            var messageList = $('#sqsMessageList');
            console.log(data)
            data.forEach(function (message) {
                console.log(message.Body)
                messageList.append("<div class='dropdown-item'>" + message.Body + '</div>');
            });
        });
        
    }, 5000); // Poll every 5 seconds
});
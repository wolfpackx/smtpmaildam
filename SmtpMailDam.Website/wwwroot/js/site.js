// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function testImapCredentials(url) {
    var host = $("#ImapHost");
    var port = $("#ImapPort");
    var ssl = $("#ImapSSLEnabled");
    var username = $("#ImapUsername");
    var password = $("#ImapPassword");

    $("#imaplogintestresult").html("<img src='/img/ajax-loader.gif' />");

    $.ajax({
        type: "POST",
        url: url,
        data: { host: host.val(), port: port.val(), ssl: ssl.val(), username: username.val(), password: password.val() },
        dataType: "json",
        success: function (response) {
            if (response.success) {
                $("#imaplogintestresult").html("Login succeeded");
            }
            else {
                $("#imaplogintestresult").html("Login failed");
            }
        },
        error: function (req, status, error) {
            $("#imaplogintestresult").html("Something went wrong. Please try again.");
        }
    });
}
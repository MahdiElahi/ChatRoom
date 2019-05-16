var current_user = "";
var target_user = "";
var chatHistory = document.getElementById("chat-box");
//Initialize SignalR
var connection = new signalR.HubConnectionBuilder()
    .withUrl('/chatHub')
    .build();
connection.on('LoadHistory', LoadHistory);
connection.on('ReceiveMessage', renderMessage);
connection.on('GetUsers', renderUsers);
connection.on('Logout', LogoutUser);
connection.on('GetUserName', GetUserName);
connection.on('GetMessageCount', renderMessageCount);

function startConnection() {
    connection.start().then(GetCurrentUserName).catch(function (err) {
        console.log(err);
    });

}
function GetCurrentUserName() {
    connection.invoke("GetUserName");
    connection.invoke("GetUsers");
}
function GetUserName(userName) {
    current_user = userName;
}
function ready() {
    //if ($("p").is(":visible")) {
    //    alert("The paragraph  is visible.");
    //} else {
    //    alert("The paragraph  is hidden.");
    //}
    startConnection();
    debugger;

    $("#first_div").removeClass('hidden');
    $("#main_div").addClass('hidden');
    debugger; logoutForm
    var chatForm = document.getElementById('chatForm');
    chatForm.addEventListener('submit',
        function (e) {
            e.preventDefault();
            var text = e.target[0].value;
            e.target[0].value = '';
            sendMessage(text);

        });
    //
    var logoutForm = document.getElementById('logoutForm');
    logoutForm.addEventListener('submit',
        function (e) {
            e.preventDefault();
            connection.invoke("Logout");
            logoutForm.submit();

        });


}
function LoadHistory(messages) {
    if (!messages.length) {
        var div_empty = '<div class="alert alert-info  col-md-12 text-center" id="div_empty" >هیچ گفتگویی ثبت نشده</div>'
        $("#chat-box").append(div_empty);
        return;
    }
    var user_image = "";
    var user_Fullname = "";
    messages.forEach(function (e) {
        if (e.userPhoto != null) { user_image = e.userPhoto; } else { user_image = "/images/no-profile.jpg"; }
        if (e.userName_1 == current_user) { user_Fullname = "شما"; } else { user_Fullname = e.fullName };
        var div_chat = '<div class="item">' +
            '<img src="' + user_image + '" alt="user image"  class="offline">'
            + '<p class="message">'
            + '<a class="name" >' +
            '<small class="text-muted pull-left"><i class="fa fa-clock"></i>&nbsp;' + e.date + ' &nbsp;|&nbsp;<i class="fa fa-clock-o"></i>&nbsp;' + e.time + '</small>'
            + '<small>' + user_Fullname + '</small>'
            + '</a>' + e.message + '</p></div>';


        $("#chat-box").append(div_chat);
    });
    debugger;
    var scr = $('#chat-box')[0].scrollHeight;
    $('#chat-box').animate({ scrollTop: scr }, 2000);
    //chatHistory.scrollTop = chatHistory.scrollHeight - chatHistory.clientHeight;
}
function sendMessage(text) {
    if (text && text.length) {
        connection.invoke('SendMessage', text, target_user);
    }
}

function renderMessage(model) {
    $("#div_empty").hide();
    debugger
    if ((model.userName_1 == current_user || model.userName_1 == target_user) && (model.userName_2 == current_user || model.userName_2 == target_user)) {
        if (model.userPhoto != null) { user_image = model.userPhoto; } else { user_image = "/images/no-profile.jpg"; }
        if (model.userName_1 == current_user) { user_Fullname = "شما"; } else { user_Fullname = model.fullName };
        var div_chat = '<div class="item">' +
            '<img src="' + user_image + '" alt="user image"  class="offline">'
            + '<p class="message">'
            + '<a class="name" >' +
            '<small class="text-muted pull-left"><i class="fa fa-clock"></i>&nbsp;' + model.date + ' &nbsp;|&nbsp;<i class="fa fa-clock-o"></i>&nbsp;' + model.time + '</small>'
            + '<small>' + user_Fullname + '</small>'
            + '</a>' + model.message + '</p></div>';

        $("#chat-box").append(div_chat);
        $("#chat-box").animate({
            scrollTop: $("#chat-box").position().top
        }, 1000);

    }
}
function renderMessageCount(m) {

}
function renderUsers(users) {
    debugger;
    document.getElementById("users-box").innerHTML = "";
    users.forEach(function myfunction(e) {
        if (e.user.userPhoto != null) { user_image = e.user.userPhoto; } else { user_image = "/images/no-profile.jpg"; }

        //
        if (e.user.userName != current_user) {
            var classOnline = "offline";
            if (e.online) classOnline = "online";

            var div_chat = '<div class="item">' +
                '<img src="' + user_image + '" id="' + e.user.userName + '" alt="user image" class="' + classOnline + '">'
                + '<p class="message">'
                + '<a class="name" style="cursor:pointer" onclick="ChangeRoom(' + "'" + e.user.userName + "'" + ')">' +
                '<small class=" pull-left"><span class="label label-danger" id="countMessage_'+e.user.userName+'"></span></small>'
                + '<small>' + e.user.fullName + '</small>'
                + '</a><small class="text-muted">آخرین بازدید : </small> </p></div>';

            $("#users-box").append(div_chat);
        }
    });

    //chatHistory.scrollTop = chatHistory.scrollHeight - chatHistory.clientHeight;
}
function LogoutUser(username) {
    debugger;
    document.getElementById(username).className = "offline";
}

document.addEventListener('DOMContentLoaded', ready);

function ChangeRoom(username) {
    target_user = username;
    $("#first_div").hide();
    $("#main_div").removeClass('hidden');
    document.getElementById("chat-box").innerHTML = "";
    ; connection.invoke("ChangeRoom", username);
}

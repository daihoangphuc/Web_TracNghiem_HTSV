// Chức năng kết nối đến ChatHub
let connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (user, message) {
    let msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    let encodedMsg = user + ": " + msg;
    let li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").append(li);
});

connection.on("LoadHistory", function (history) {
    // Chú ý đoạn này cập nhật lịch sử vào UI
    let formattedHistory = history.replace(/\n/g, "<br>"); // Chuyển ký tự xuống dòng thành thẻ <br>
    document.getElementById("messagesList").innerHTML = formattedHistory;
});

// Kết nối
connection.start().catch(function (err) {
    return console.error(err.toString());
});

// Hàm gửi tin nhắn
document.getElementById("sendButton").addEventListener("click", function (event) {
    let user = document.getElementById("userInput").value;
    let message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});











//OLD
/*const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveMessage", function (user, message) {
    const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    const encodedMsg = user + " : " + msg;
    const li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.addEventListener("DOMContentLoaded", function () {
    const sendButton = document.getElementById("sendButton");
    sendButton.addEventListener("click", function (event) {
        const user = document.getElementById("userInput").value;
        const message = document.getElementById("messageInput").value;
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });
        document.getElementById("messageInput").value = '';
        event.preventDefault();
    });
});*/
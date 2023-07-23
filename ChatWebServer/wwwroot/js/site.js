document.addEventListener("DOMContentLoaded", function () {
    var protocol = location.protocol === "https:" ? "wss:" : "ws:";
    var wsUri = protocol + "//" + window.location.host;
    var socket = new WebSocket(wsUri);

    const d = new Date();
    let time = d.getTime();

    socket.onopen = function (e) {
        console.log("socket opened", e);
    };

    socket.onclose = function (e) {
        console.log("socket closed", e);
    };

    socket.onmessage = function (e) {
        console.log(e);
        var messageDiv = document.createElement("div");
        messageDiv.className = "message"; 
        messageDiv.textContent = e.data;
        document.getElementById("msgs").appendChild(messageDiv);
    };

    socket.onerror = function (e) {
        console.error(e.data);
    };

    document.getElementById("messageField").addEventListener("keypress", function (e) {
        if (e.key !== "Enter") {
            return;
        }

        e.preventDefault();

        var message = userName + ": " + this.value;
        var messageValue = this.value;
        socket.send(message);

        message = messageValue.substr(messageValue.indexOf(":") + 2); 
        saveMessageToDb(message);
        this.value = "";
    });

    function saveMessageToDb(message) {
        var formData = new FormData();
        formData.append("message", message)

        fetch("https://chat.mregli.com/Home/SaveMessage", {
            method: 'POST',
            body: formData
        })
            .then(response => {
                console.log(response.status + " " + response.statusText)
            })
            .catch(error => {
                console.error("Error saving message: ", error)
            })
    }
});
document.addEventListener("DOMContentLoaded", function () {
    var protocol = location.protocol === "https:" ? "wss:" : "ws:";
    var wsUri = protocol + "//" + window.location.host;
    var socket = new WebSocket(wsUri);

    const d = new Date();
    let time = d.getTime();

    socket.onopen = function (e) {
        console.log("socket opened", e);
        fetchLastMessages(10);
    };

    socket.onclose = function (e) {
        console.log("socket closed", e);
    }

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

        if (!userName) {
            // If userName is not available, the user is not authenticated
            console.error("User is not authenticated.");
            return;
        }

        var message = userName + ": " + this.value;
        var messageValue = this.value;
        socket.send(message);

        message = messageValue.substr(messageValue.indexOf(":") + 1); 
        saveMessageToDb(message);
        this.value = "";
    });

    function saveMessageToDb(message) {
        var formData = new FormData();
        formData.append("message", message)

        fetch("https://chat-test-chatweb.mregli.com/Home/SaveMessage", {
            method: 'POST',
            body: formData,
            credentials: 'include'
        })
            .then(response => {
                console.log(response.status + " " + response.statusText)
            })
            .catch(error => {
                console.error("Error saving message: ", error)
            });
    }

    function fetchLastMessages(count) {
        var formData = new FormData();
        formData.append("count", count);

        fetch("https://chat-test-chatweb.mregli.com/Home/GetLastMessages", {
            method: 'POST',
            body: formData,
            credentials: 'include'
        })
            .then(response => response.json())
            .then(data => {
                // Display the last messages in the chat window
                data.reverse().forEach(message => {
                    var messageDiv = document.createElement("div");
                    messageDiv.className = "message";
                    messageDiv.textContent = `${message.Username}: ${message.Message} (${message.Timestamp})`;
                    document.getElementById("msgs").appendChild(messageDiv);
                });
            })
            .catch(error => {
                console.error("Error fetching last messages: ", error);
            });
    }



});
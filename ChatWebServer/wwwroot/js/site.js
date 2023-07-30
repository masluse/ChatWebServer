document.addEventListener("DOMContentLoaded", function () {
    var protocol = location.protocol === "https:" ? "wss:" : "ws:";
    var wsUri = protocol + "//" + window.location.host;
    var socket = new WebSocket(wsUri);
    const logoutBtn = document.getElementById("logoutBtn");

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
        var messageData = JSON.parse(e.data);
        var username = messageData.username;
        var role = messageData.role;
        var message = messageData.message;
        var timestamp = messageData.timestamp;

        var messageDiv = document.createElement("div");
        messageDiv.className = "message";

        // Apply red color to messages from users with the "ADMIN" role
        var nameElement = document.createElement("span");
        nameElement.className = role === "ADMIN" ? "username admin" : "username";
        nameElement.textContent = username;

        var timeElement = document.createElement("span");
        timeElement.className = "timestamp";
        timeElement.textContent = formatTimestamp(timestamp);

        var contentElement = document.createElement("p");
        contentElement.textContent = message;

        messageDiv.appendChild(nameElement);
        messageDiv.appendChild(timeElement);
        messageDiv.appendChild(contentElement);

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

        fetch("/Home/SaveMessage", {
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

        fetch("/Home/GetLastMessages", {
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
                    messageDiv.textContent = `${message.username}: ${message.message}`;
                    document.getElementById("msgs").appendChild(messageDiv);
                });
            })
            .catch(error => {
                console.error("Error fetching last messages: ", error);
            });
    }

    logoutBtn.addEventListener("click", function () {
        // Send a POST request to the Logout action
        fetch("/Home/Logout", {
            method: "POST",
            headers: {
                "Content-Type": "application/json" // Set the content type if required
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("An error occurred while logging out.");
                }
                // If the logout was successful, redirect the user to the login page
                window.location.href = "/Home/Index";
                showSuccessToast("Loged out")
            })
            .catch(error => {
                // Handle any errors if needed
                console.error(error.message);
                showErrorToast("An error occurred while logging out.");
            });
    });

    // Toasts 

    // Function to show success toast
    function showSuccessToast(message) {
        Toastify({
            text: message,
            backgroundColor: "green",
            duration: 3000,
            close: true,
            gravity: "top",
            position: "right",
        }).showToast();
    }

    // Function to show error toast
    function showErrorToast(message) {
        Toastify({
            text: message,
            backgroundColor: "red",
            duration: 3000,
            close: true,
            gravity: "top",
            position: "right",
        }).showToast();
    }

});
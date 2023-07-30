﻿document.addEventListener("DOMContentLoaded", function () {
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
        var receivedData = e.data;
        var separatorIndex = receivedData.indexOf(":");
        var receivedUsername = receivedData.substring(0, separatorIndex);
        var receivedMessage = receivedData.substring(separatorIndex + 1);

        var messageDiv = document.createElement("div");
        messageDiv.className = "message";

        // Add a specific class for the message based on the user's role
        if (receivedUsername.includes("[ADMIN]")) {
            messageDiv.classList.add("admin-message");
        }

        messageDiv.textContent = receivedUsername + ": " + receivedMessage;
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
                data.reverse().forEach(messageData => {
                    var username = messageData.username;
                    var role = messageData.role;
                    var message = messageData.message;

                    var messageDiv = document.createElement("div");
                    messageDiv.className = "message";

                    // Create a span element for the username
                    var nameElement = document.createElement("span");

                    // Set the class to "admin" if the user has the "ADMIN" role
                    if (role === "ADMIN") {
                        nameElement.classList.add("admin");
                    }

                    nameElement.textContent = username + ": ";

                    var contentElement = document.createElement("span");
                    contentElement.textContent = message;

                    messageDiv.appendChild(nameElement);
                    messageDiv.appendChild(contentElement);

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
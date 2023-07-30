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
        var messageData = e.data.split(":");
        var username = messageData[0].trim();
        var message = messageData.slice(1).join(":").trim();

        var messageDiv = document.createElement("div");
        messageDiv.className = "message";

        var usernameSpan = document.createElement("span");
        usernameSpan.textContent = username + ": ";

        // Get the role of the user from userListData
        var role = getUserRole(username);
        if (role === "ADMIN") {
            usernameSpan.classList.add("admin");
        }

        var messageSpan = document.createElement("span");
        messageSpan.textContent = message;

        messageDiv.appendChild(usernameSpan);
        messageDiv.appendChild(messageSpan);

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
        fetch("/Home/Logout", {
            method: "POST",
            headers: {
                "Content-Type": "application/json" 
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("An error occurred while logging out.");
                }
                window.location.href = "/Home/Index";
                showSuccessToast("Loged out")
            })
            .catch(error => {
                console.error(error.message);
                showErrorToast("An error occurred while logging out.");
            });
    });


    function getUserRole(username) {
        var user = userListData.find(user => user.Username === username);

        return user ? user.Role : "";
    }

    // Toasts 

    // Function to show success toast
    function showSuccessToast(message) {
        Toastify({
            text: message,
            backgroundColor: "green",
            duration: 3000,
            close: true,
            gravity: "top",
            position: "center",
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
            position: "center",
        }).showToast();
    }

});
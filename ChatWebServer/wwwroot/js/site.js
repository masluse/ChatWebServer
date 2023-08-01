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

        if (currentUser.Role === "ADMIN") {
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

        if (!currentUser) {
            console.error("User is not authenticated.");
            return;
        }

        var message = currentUser.Username + ": " + this.value;
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

function manageSideBar() {
    let chats = document.getElementsByClassName("chat-box");
    let mainElement = document.getElementById("main");
    let sidenavElement = document.getElementById("mySidenav");
    let inputBox = document.getElementById("inputBox");

    if (mainElement.style.marginLeft === "0px") {
        sidenavElement.style.width = "250px";
        mainElement.style.marginLeft = "250px";
        inputBox.style.marginLeft = "250px";
    } else {
        sidenavElement.style.width = "0";
        mainElement.style.marginLeft = "0";
        inputBox.style.marginLeft = "0";
    }
}

const openNewChatDialogBtn = document.getElementById("createChatBtn");
const newChatDialog = document.getElementById("newChatDialog");

openNewChatDialogBtn.addEventListener("click", () => {
    // Fetch the list of users from the server
    fetch("/Home/GetAllUsers")
        .then((response) => response.json())
        .then((data) => {
            // Clear any existing checkboxes in the dialog
            const userListContainer = document.getElementById("userList");
            userListContainer.innerHTML = "";

            // Create a checkbox for each user
            data.forEach((user) => {
                const userBox = document.createElement("div");
                userBox.classList.add("user-box");

                const input = document.createElement("input");
                input.type = "checkbox";
                input.id = `userCheckbox_${user.userID}`;
                input.value = user.username;

                const label = document.createElement("label");
                label.textContent = user.username;
                label.htmlFor = `userCheckbox_${user.userID}`;

                userBox.appendChild(input);
                userBox.appendChild(label);
                userListContainer.appendChild(userBox);
            });

            // Show the dialog
            newChatDialog.showModal();
        })
        .catch((error) => {
            console.error("Error fetching users: ", error);
        });

});

function closeCreateNewChatDialog() {
    newChatDialog.close();
}

function createNewChat() {
    const userCheckboxes = document.querySelectorAll(".user-checkbox");

    const checkedUsernames = [];

    userCheckboxes.forEach((checkbox) => {
        if (checkbox.checked) {
            const username = checkbox.dataset.username;
            checkedUsernames.push(username);
        }
    });

    console.log("Checked Users:", checkedUsernames);
}
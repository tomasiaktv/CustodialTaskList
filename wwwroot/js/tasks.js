
let currentShift = "Morning";
let selectedTaskId = null;
let currentTasks = [];

const modalElement = document.getElementById("initialsModal");
const initialsModal = new bootstrap.Modal(modalElement);

// Display the full date and weekday.
function updateDate() {
    document.getElementById("currentDate").textContent =
        new Date().toLocaleDateString("en-US", {
            weekday: "long",
            month: "long",
            day: "numeric",
            year: "numeric"
        });
}

// Request tasks from the C# server.
async function loadTasks(shift) {
    currentShift = shift;

    document.getElementById("shiftTitle").textContent =
        `${shift} Shift`;

    document.getElementById("morningButton").className =
        shift === "Morning"
            ? "btn btn-primary flex-fill"
            : "btn btn-outline-primary flex-fill";

    document.getElementById("eveningButton").className =
        shift === "Evening"
            ? "btn btn-primary flex-fill"
            : "btn btn-outline-primary flex-fill";

    const container = document.getElementById("taskList");
    container.textContent = "Loading tasks...";

    try {
        const response = await fetch(
            `/?handler=Tasks&shift=${encodeURIComponent(shift)}`,
            { cache: "no-store" }
        );

        if (!response.ok) {
            throw new Error("Unable to retrieve tasks.");
        }

        const tasks = await response.json();

        // Ignore an older response if the shift changed.
        if (currentShift !== shift) return;

        currentTasks = tasks;
        renderTasks();
    } catch (error) {
        container.textContent = error.message;
    }
}

// Render tasks returned by the server.
function renderTasks() {
    const container = document.getElementById("taskList");
    container.replaceChildren();

    const completed = currentTasks.filter(
        task => task.completed
    ).length;

    document.getElementById("progressCounter").textContent =
        `${completed} / ${currentTasks.length} Complete`;

    if (currentTasks.length === 0) {
        container.textContent =
            "No tasks scheduled for this shift today.";
        return;
    }

    currentTasks.forEach(task => {
        const card = document.createElement("div");
        card.className = "card mb-3";

        const body = document.createElement("div");
        body.className = "card-body";

        const heading = document.createElement("h5");
        heading.textContent = task.name;
        const header = document.createElement("div");
        header.className = "task-card-header";
        header.appendChild(heading);

        if (task.room && task.room.trim()) {
            const roomBlock = document.createElement("div");
            roomBlock.className = "task-room";
            const roomLabel = document.createElement("span");
            roomLabel.className = "task-room-label";
            roomLabel.textContent = "Room";
            const roomValue = document.createElement("strong");
            roomValue.className = "task-room-value";
            roomValue.textContent = task.room;
            roomBlock.append(roomLabel, roomValue);
            header.appendChild(roomBlock);
        }
        body.appendChild(header);


        if (task.completed) {
            card.classList.add("border-success");

            const status = document.createElement("p");
            status.className = "text-success mb-2";
            status.textContent =
                `Completed by ${task.initials} at ${task.completedAt}`;

            body.appendChild(status);

            // Create the Undo Completion button.
            const undoButton = document.createElement("button");

            undoButton.type = "button";
            undoButton.className = "btn btn-outline-warning mt-2";
            undoButton.textContent = "↶ Undo Completion";

            // Connect the button to our Undo function.
            undoButton.addEventListener("click", () => {
                undoCompletion(task.id, undoButton);
            });

            body.appendChild(undoButton);
        } else {

            const button = document.createElement("button");
            button.className = "btn btn-success btn-lg mt-2";
            button.textContent = "Complete";

            button.addEventListener("click", () => {
                openCompletionPopup(task.id);
            });

            body.appendChild(button);
        }

        card.appendChild(body);
        container.appendChild(card);
    });
}

// Open the employee initials popup.
function openCompletionPopup(taskId) {
    selectedTaskId = taskId;

    const task = currentTasks.find(t => t.id === taskId);
    if (!task || task.completed) return;

    document.getElementById("selectedTaskName").textContent =
        task.name;

    document.getElementById("employeeInitials").value = "";

    buildKeyboard();
    initialsModal.show();
}

// Create the touchscreen keyboard.
function buildKeyboard() {
    const keyboard = document.getElementById("virtualKeyboard");
    keyboard.replaceChildren();

    const rows = [
        "QWERTYUIOP",
        "ASDFGHJKL",
        "ZXCVBNM"
    ];

    rows.forEach(row => {
        const rowElement = document.createElement("div");
        rowElement.className =
            "d-flex justify-content-center gap-1 mb-2";

        for (const letter of row) {
            const key = document.createElement("button");

            key.type = "button";
            key.className =
                "btn btn-outline-dark keyboard-key";
            key.textContent = letter;

            key.addEventListener("click", () => {
                const input =
                    document.getElementById("employeeInitials");

                if (input.value.length < 5) {
                    input.value += letter;
                }
            });

            rowElement.appendChild(key);
        }

        keyboard.appendChild(rowElement);
    });

    const backspace = document.createElement("button");
    backspace.type = "button";
    backspace.className = "btn btn-warning mt-2";
    backspace.textContent = "Backspace";

    backspace.addEventListener("click", () => {
        const input =
            document.getElementById("employeeInitials");

        input.value = input.value.slice(0, -1);
    });

    // Create a bottom row for Backspace and &.
    const bottomRow = document.createElement("div");
    bottomRow.className = "d-flex align-items-center mt-2";
    bottomRow.style.position = "relative";
    bottomRow.style.minHeight = "48px";

    // Backspace stays on the left.
    bottomRow.appendChild(backspace);

    // Create the & button.
    const andSymbol = document.createElement("button");
    andSymbol.type = "button";
    andSymbol.className = "btn btn-outline-dark keyboard-key";
    andSymbol.textContent = "&";
    andSymbol.style.minWidth = "65px"

    // Center the & button horizontally.
    andSymbol.style.position = "absolute";
    andSymbol.style.left = "50%";
    andSymbol.style.transform = "translateX(-50%)";

    andSymbol.addEventListener("click", () => {
        const input = document.getElementById("employeeInitials");

        if (input.value.length < 5 && !input.value.includes("&")) {
            input.value += "&";
        }
    });

    bottomRow.appendChild(andSymbol);
    keyboard.appendChild(bottomRow);
}

// Submit task completion to SQLite through C#.
async function confirmCompletion() {
    if (selectedTaskId === null) return;

    const initials = document
        .getElementById("employeeInitials")
        .value.trim();

    if (!/^(?:[A-Z]{1,4}|[A-Z]{1,2}&[A-Z]{1,2})$/.test(initials)) {
        alert("Please enter your initials.");
        return;
    }

    const button = document.getElementById("confirmButton");
    button.disabled = true;

    const token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
    ).value;

    const data = new URLSearchParams();
    data.append("taskId", selectedTaskId);
    data.append("initials", initials);
    data.append("__RequestVerificationToken", token);

    try {
        const response = await fetch("/?handler=Complete", {
            method: "POST",
            headers: {
                "Content-Type":
                    "application/x-www-form-urlencoded"
            },
            body: data.toString()
        });

        if (!response.ok) {
            throw new Error(
                response.status === 409
                    ? "This task was already completed."
                    : "Unable to save the completion."
            );
        }

        initialsModal.hide();
        selectedTaskId = null;

        // Reload the saved records from SQLite.
        await loadTasks(currentShift);

    } catch (error) {
        alert(error.message);
    } finally {
        button.disabled = false;
    }
}

async function undoCompletion(taskId, button) {

    // Ask the employee to confirm the action.
    const confirmed = confirm(
        "Are you sure you want to undo this task completion?"
    );

    if (!confirmed) {
        return;
    }

    // Prevent double-clicking during the request.
    button.disabled = true;

    // Get the security token from our Razor form.
    const token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
    ).value;

    // Prepare the task ID and security token.
    const data = new URLSearchParams();

    data.append("taskId", taskId);
    data.append("__RequestVerificationToken", token);

    try {
        // Send the Undo request to our C# method.
        const response = await fetch("/?handler=Undo", {
            method: "POST",
            headers: {
                "Content-Type":
                    "application/x-www-form-urlencoded"
            },
            body: data.toString()
        });

        if (!response.ok) {
            throw new Error(
                response.status === 409
                    ? "This task is already incomplete."
                    : "Unable to undo task completion."
            );
        }

        // Reload the tasks from the database.
        await loadTasks(currentShift);

    } catch (error) {
        alert(error.message);
    } finally {
        button.disabled = false;
    }
}

// based on the kiosk's local time.
function getScheduledShift() {
    const hour = new Date().getHours();

    // Morning: 6:00 AM - 1:59 PM
    if (hour >= 6 && hour < 14) {
        return "Morning";
    }

    // Evening: 2:00 PM - 9:59 PM
    if (hour >= 14 && hour < 22) {
        return "Evening";
    }

    // Outside working hours, default to Morning.
    return "Morning";
}


// Track the shift that should currently be displayed.
let lastScheduledShift = getScheduledShift();

// Load the correct shift when the page opens.
updateDate();
loadTasks(lastScheduledShift);

// Automatically refresh every 10 seconds.
setInterval(async () => {

    // Don't interrupt an employee entering initials.
    // Your actual modal ID is "initialsModal".
    if (modalElement.classList.contains("show")) {
        return;
    }

    // Update the displayed date.
    updateDate();

    // Check which shift should be displayed.
    const scheduledShift = getScheduledShift();

    // Automatically change shifts when the
    // scheduled shift changes.
    if (scheduledShift !== lastScheduledShift) {

        lastScheduledShift = scheduledShift;

        // Switch to the new shift.
        await loadTasks(scheduledShift);

        return;
    }

    // Don't refresh a hidden browser tab.
    if (document.hidden) {
        return;
    }

    // Otherwise, refresh the currently selected shift.
    await loadTasks(currentShift);

}, 10000);
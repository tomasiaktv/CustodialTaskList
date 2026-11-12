
document.addEventListener("DOMContentLoaded", () => {
    const keyboard = document.getElementById(
        "adminVirtualKeyboard"
    );

    const keysContainer = document.getElementById(
        "keyboardKeys"
    );

    const closeButton = document.getElementById(
        "closeKeyboard"
    );

    const keyboardHeader = keyboard?.querySelector(
        ".keyboard-header"
    );

    if (!keyboard || !keysContainer || !closeButton) {
        return;
    }

    let activeInput = null;
    let isShift = false;
    let isSymbols = false;
    let wasDragged = false;

    const letters = [
        ["q", "w", "e", "r", "t", "y", "u", "i", "o", "p"],
        ["a", "s", "d", "f", "g", "h", "j", "k", "l"],
        ["Shift", "z", "x", "c", "v", "b", "n", "m", "Del"],
        ["?123", ",", "Space", ".", "Done"]
    ];

    const symbols = [
        ["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"],
        ["@", "#", "$", "%", "&", "-", "+", "(", ")", "/"],
        ["Shift", "*", "\"", "'", ":", ";", "!", "?", "Del"],
        ["ABC", "_", "Space", "=", "Done"]
    ];

    function createKey(label) {
        const button = document.createElement("button");

        button.type = "button";
        button.className = "keyboard-key";
        button.textContent =
            isShift && label.length === 1 && !isSymbols
                ? label.toUpperCase()
                : label;

        if (label === "Space") {
            button.classList.add("keyboard-space");
        }

        if (["Shift", "Del", "Done",
            "?123", "ABC"].includes(label)) {
            button.classList.add("keyboard-special");
        }

        // Prevent the input from losing focus
        // when using the keyboard with a mouse.
        button.addEventListener("pointerdown", event => {
            event.preventDefault();
        });

        button.addEventListener("click", () => {
            handleKey(label);
        });

        return button;
    }

    function renderKeyboard() {
        keysContainer.replaceChildren();

        const layout = isSymbols ? symbols : letters;

        layout.forEach(rowKeys => {
            const row = document.createElement("div");
            row.className = "keyboard-row";

            rowKeys.forEach(label => {
                row.appendChild(createKey(label));
            });

            keysContainer.appendChild(row);
        });
    }

    function insertText(text) {
        if (!activeInput) return;

        const start = activeInput.selectionStart;
        const end = activeInput.selectionEnd;
        const value = activeInput.value;

        activeInput.value =
            value.substring(0, start) +
            text +
            value.substring(end);

        const cursor = start + text.length;

        activeInput.setSelectionRange(cursor, cursor);
        activeInput.dispatchEvent(
            new Event("input", { bubbles: true })
        );

        activeInput.focus();
    }

    function handleKey(label) {
        if (!activeInput) return;

        switch (label) {
            case "Done":
                hideKeyboard();
                return;

            case "Shift":
                isShift = !isShift;
                renderKeyboard();
                return;

            case "?123":
                isSymbols = true;
                renderKeyboard();
                return;

            case "ABC":
                isSymbols = false;
                renderKeyboard();
                return;

            case "Space":
                insertText(" ");
                return;

            case "Del": {
                const start = activeInput.selectionStart;
                const end = activeInput.selectionEnd;

                if (start !== end) {
                    activeInput.setRangeText(
                        "", start, end, "end"
                    );
                } else if (start > 0) {
                    activeInput.setRangeText(
                        "", start - 1, start, "end"
                    );
                }

                activeInput.dispatchEvent(
                    new Event("input", { bubbles: true })
                );
                activeInput.focus();
                return;
            }

            default:
                insertText(
                    isShift && !isSymbols
                        ? label.toUpperCase()
                        : label
                );

                if (isShift) {
                    isShift = false;
                    renderKeyboard();
                }
        }
    }

    function positionKeyboard(input) {
        const inputRect = input.getBoundingClientRect();

        const keyboardWidth = keyboard.offsetWidth;
        const keyboardHeight = keyboard.offsetHeight;

        const margin = 10;
        const gap = 12;

        // Center the keyboard below the selected input.
        let left = inputRect.left +
            (inputRect.width - keyboardWidth) / 2;

        let top = inputRect.bottom + gap;

        // If there isn't enough room below,
        // try positioning above the input.
        if (top + keyboardHeight >
            window.innerHeight - margin) {

            const above = inputRect.top -
                keyboardHeight - gap;

            if (above >= margin) {
                top = above;
            } else {
                // Fall back to the lower third,
                // keeping the keyboard on-screen.
                top = Math.max(
                    margin,
                    window.innerHeight - keyboardHeight - margin
                );
            }
        }

        // Prevent the keyboard from leaving the screen.
        left = Math.max(
            margin,
            Math.min(
                left,
                window.innerWidth - keyboardWidth - margin
            )
        );

        top = Math.max(
            margin,
            Math.min(
                top,
                window.innerHeight - keyboardHeight - margin
            )
        );

        keyboard.style.left = `${left}px`;
        keyboard.style.top = `${top}px`;
    }

    function showKeyboard(input) {
        const wasHidden =
            keyboard.style.display === "none";

        activeInput = input;
        isShift = false;
        isSymbols = false;

        renderKeyboard();
        keyboard.style.display = "block";

        // Reposition only when opening for the first
        // time or when it hasn't been manually moved.
        if (wasHidden || !wasDragged) {
            requestAnimationFrame(() => {
                if (!wasDragged) {
                    positionKeyboard(input);
                }
            });
        }
    }

    function hideKeyboard() {
        keyboard.style.display = "none";
        activeInput = null;

        // Reset position preference when closed.
        wasDragged = false;
    }

    // Only inputs specifically marked for
    // the Admin keyboard will activate it.
    document.querySelectorAll(
        ".admin-keyboard-input"
    ).forEach(input => {
        input.addEventListener("focus", () => {
            showKeyboard(input);
        });
    });

    // Allow the Admin keyboard to be dragged
    // using either a touchscreen or mouse.
    if (keyboardHeader) {

        let dragging = false;
        let offsetX = 0;
        let offsetY = 0;

        keyboardHeader.addEventListener(
            "pointerdown", (event) => {

                // Don't drag when pressing the Done button.
                if (event.target.closest("button")) {
                    return;
                }

                dragging = true;

                const rect = keyboard.getBoundingClientRect();

                offsetX = event.clientX - rect.left;
                offsetY = event.clientY - rect.top;

                keyboardHeader.setPointerCapture(
                    event.pointerId
                );
            });

        keyboardHeader.addEventListener(
            "pointermove", (event) => {

                if (!dragging) return;
                wasDragged = true;

                const margin = 10;

                let left = event.clientX - offsetX;
                let top = event.clientY - offsetY;

                // Keep the keyboard inside the viewport.
                left = Math.max(
                    margin,
                    Math.min(
                        left,
                        window.innerWidth -
                        keyboard.offsetWidth - margin
                    )
                );

                top = Math.max(
                    margin,
                    Math.min(
                        top,
                        window.innerHeight -
                        keyboard.offsetHeight - margin
                    )
                );

                keyboard.style.left = `${left}px`;
                keyboard.style.top = `${top}px`;
            });

        function stopDragging() {
            dragging = false;
        }

        keyboardHeader.addEventListener(
            "pointerup", stopDragging
        );

        keyboardHeader.addEventListener(
            "pointercancel", stopDragging
        );
    }

    closeButton.addEventListener("click", hideKeyboard);
});

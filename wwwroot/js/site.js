// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function logButtonClick(buttonType) {
    const data = {
        buttonType: buttonType
        // UserId and SessionId will be added on the backend if available
    };

    fetch('/api/Log/click', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    }).then(response => {
        if (!response.ok) {
            console.error('Failed to log button click:', response.statusText);
        }
    }).catch(error => {
        console.error('Error logging button click:', error);
    });
}

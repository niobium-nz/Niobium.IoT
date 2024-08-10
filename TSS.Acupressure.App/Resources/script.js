function getAvailableNetworks() {
    return Promise.race([
        fetch('/network-available'),
        new Promise((_, reject) =>
            setTimeout(() => reject(new Error('Timeout')), 10000)
        )
    ])
    .then(response => response.json())
    .then(data => {
        const result = [];
        data.forEach((item, row) => {
            result.push({
                ssid: item.SSID,
                signal: getSignal(item.RSSI)
            });
        });
        return result.sort((a, b) => b.signal - a.signal);
    });
}

function getSignal(rssi) {
    var percentage;
    if (rssi <= -100) {
        percentage = 0;
    } else if (rssi >= -50) {
        percentage = 100;
    } else {
        percentage = 2 * (rssi + 100);
    }
    var rounded = Math.round(percentage * 10) / 10
    if (rounded < 10) {
        return 0;
    } else if (rounded >= 10 && rounded < 35) {
        return 1;
    } else if (rounded >= 35 && rounded < 60) {
        return 2;
    } else if (rounded >= 60 && rounded < 90) {
        return 3;
    } else if (rounded >= 90) {
        return 4;
    }

    return 0;
}

// Function to populate the dropdown with available networks and their signal strengths
async function populateNetworks() {
    const ssidSelect = document.getElementById('ssid');
    const loadingNetworks = document.getElementById('loading-networks');
    const wifiForm = document.getElementById('wifi-form');

    // Show loading indicator
    loadingNetworks.style.display = 'block';

    try {
        const networks = await getAvailableNetworks();

        networks.forEach(network => {
            const option = document.createElement('option');
            option.value = network.ssid;
            option.textContent = network.ssid;

            const signalStrength = document.createElement('span');
            signalStrength.className = 'signal-strength';
            signalStrength.innerHTML = getSignalStrengthIcon(network.signal);

            option.appendChild(signalStrength);
            ssidSelect.appendChild(option);
        });
    } catch (error) {
        loadingNetworks.textContent = 'Failed to load networks. Please try again.';
    } finally {
        // Hide loading indicator and show form
        loadingNetworks.style.display = 'none';
        wifiForm.style.display = 'block';
    }
}

// Function to return signal strength icons based on signal level
function getSignalStrengthIcon(signal) {
    if (signal >= 4) {
        return ' 🔵🔵🔵🔵🔵';  // Excellent signal
    } else if (signal >= 3) {
        return ' 🔵🔵🔵🔵⚪';  // Good signal
    } else if (signal >= 2) {
        return ' 🔵🔵🔵⚪⚪';  // Fair signal
    } else if (signal >= 1) {
        return ' 🔵🔵⚪⚪⚪';  // Weak signal
    } else {
        return ' ⚪⚪⚪⚪⚪';  // No signal
    }
}

// Function to validate the WiFi password
function validatePassword(password) {
    if (password.length < 8 || password.length > 63) {
        alert('Password must be between 8 and 63 characters long.');
        return false;
    }
    return true;
}

// Event listener for form submission
document.getElementById('wifi-form').addEventListener('submit', function (event) {
    event.preventDefault();

    const selectedNetwork = document.getElementById('ssid').value;
    const networkPassword = document.getElementById('password').value;
    const pin = document.getElementById('pin').value;
    const connectButton = this.querySelector('button[type="submit"]');

    // Validate the password length
    if (!validatePassword(networkPassword)) {
        return;
    }

    // Disable the connect button, show loading indicator, and change button text
    connectButton.disabled = true;
    connectButton.innerHTML = 'Connecting...';

    var body = `ssid=${selectedNetwork}&pwd=${networkPassword}&pin=${pin}`;
    Promise.race([
        fetch('/setup', {
            method: 'POST',
            body: body
        }),
        new Promise((_, reject) =>
            setTimeout(() => reject(new Error('Timeout')), 20000)
        )
    ])
    .then(response => {
        if (!response.ok) {
            alert(response.body);
        } else {
            alert('WiFi has been setup successfully. Please power-off the device and power it back on to continue.');
        }
    })
    .catch(error => {
        alert('Request has failed. Please retry.');
    })
    .finally(() => {
        // Re-enable the button and revert button text
        connectButton.disabled = false;
        connectButton.innerHTML = 'Connect';

        // Reset the form
        this.reset();
    });
});

// Initialize the dropdown on page load
document.addEventListener('DOMContentLoaded', populateNetworks);

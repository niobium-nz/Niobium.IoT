document.addEventListener('DOMContentLoaded', function () {
    refreshNetwork();
});

function showMessage(msg) {
    document.getElementById('dlgTitle').innerText = 'Information';
    document.querySelector(".dialog-overlay").style.display = 'flex';
    document.getElementById('dlgMessageOption').style.display = 'none';
    document.getElementById('dlgMessage').innerText = msg;
}

function hideMessage() {
    document.querySelector(".dialog-overlay").style.display = 'none';
}

function showProgressRing() {
    document.getElementById('loader-wrapper').style.display = 'block';
}

function hideProgressRing() {
    document.getElementById('loader-wrapper').style.display = 'none';
}

function clearTable() {
    document.querySelector("tbody").innerHTML = '';
}

function visiblity(tagId, isDisabled) {
    const tag = document.getElementById(tagId);
    if (tag) {
        tag.disabled = isDisabled;
    }
}

function connectNetwork() {
    ssid = document.getElementById('ssid').value;
    pass = document.getElementById('password').value;
    if (!ssid || pass.length < 8) {
        alert('Invalid input detected. Please retry.');
        return;
    }
    showProgressRing();
    visiblity('btnConnect', true);
    var body = `ssid=${ssid}&pwd=${pass}&pin=123456`;
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
                showMessage(response.body);
            } else {
                showMessage('WiFi has been setup successfully. Please unplug the power cable and replug it back in to continue.');
            }
        })
        .catch(error => {
            showMessage('Request has failed. Please retry.');
        })
        .finally(() => {
            hideProgressRing();
            visiblity('btnConnect', false);
        });
}

function refreshNetwork() {
    showProgressRing();
    visiblity('btnRefresh', true);
    fetchData('/network-available', 15000)
        .then(response => response.json())
        .then(data => {
            clearTable();
            data.forEach((item, row) => {
                addWiFi(++row, item.SSID, item.RSSI, item.BSSID);
            });
        })
        .catch(() => {
            showMessage("Unable to make a request to scan for network available. Please retry.");
        })
        .finally(() => {
            hideProgressRing();
            visiblity('btnRefresh', false);
        });
}

function settings() {
    document.getElementById('dlgTitle').innerText = 'Settings';
    document.getElementById('dlgMessage').innerText = '';
    document.getElementById('dlgMessageOption').style.display = 'block';
    document.querySelector(".dialog-overlay").style.display = 'flex';
}

function onClickItemTable(x) {
    x.classList.add('selected');
    siblings = Array.from(x.parentNode.children);
    siblings.forEach((sibling) => {
        if (sibling !== x) {
            sibling.classList.remove('selected');
        }
    });
    var value = x.querySelector('td p').textContent;
    if (value) {
        if (value !== '*HIDDEN*')
            document.getElementById('ssid').value = value;
        var passwordTag = document.getElementById('password');
        passwordTag.value = '';
        passwordTag.focus();
    }
}

function addWiFi(row, ssid, rssi, bssid) {
    var newRow = document.createElement('tr');
    newRow.onclick = function () {
        onClickItemTable(this);
    };
    var th = document.createElement('th');
    var td1 = document.createElement('td');
    var p = document.createElement('p');
    var td2 = document.createElement('td');
    var td3 = document.createElement('td');
    th.textContent = row;
    newRow.appendChild(th);
    p.textContent = ssid ? ssid : '*HIDDEN*';
    td1.appendChild(p);
    newRow.appendChild(td1);
    td2.textContent = getSignal(rssi);
    newRow.appendChild(td2);
    td3.textContent = bssid;
    newRow.appendChild(td3);
    document.getElementById('table-body').appendChild(newRow);
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
    return `${rounded}%`;
}

const fetchData = (url, timeout = 5000) => {
    return Promise.race([
        fetch(url),
        new Promise((_, reject) =>
            setTimeout(() => reject(new Error('Timeout')), timeout)
        )
    ]);
};
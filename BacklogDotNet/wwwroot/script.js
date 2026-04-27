const dashboardView = document.getElementById('dashboardView');
const emailInput = document.getElementById('email');
const passwordInput = document.getElementById('password');
let accessToken;
let transformedData = {games: [], movies: [], tv: []};
let activeDropdown = null;
let draggedItemInfo = null;

if (window.localStorage.getItem('accessToken')) {
    accessToken = window.localStorage.getItem('accessToken');
    showDashboard();
}


window.addEventListener('click', (e) => {
    if (!e.target.closest('.actions-cell') && activeDropdown) {
        activeDropdown.classList.remove('show');
        activeDropdown = null;
    }
});



function getBacklog() {
    sendHttpRequest('GET', 'http://localhost:8080/items').then((backlogData) => {
        const data = Array.isArray(backlogData) ? backlogData : [];
        transformedData = data.reduce((acc, cur) => {
            if (!(cur.mediaCategory in acc)) {
                acc[cur.mediaCategory] = [];
            }
            acc[cur.mediaCategory].push(cur);
            return acc;
        }, {games: [], movies: [], tv: []});


        Object.keys(transformedData).forEach(cat => {
            transformedData[cat].sort((a, b) => (a.ordinate || 0) - (b.ordinate || 0));
        });

        renderBacklog();
    });
}

function renderBacklog() {
    ['games', 'movies', 'tv'].forEach(mediaCategory => {
        const container = document.getElementById(`${mediaCategory}Body`);
        const countLabel = document.getElementById(`${mediaCategory}Count`);
        const items = transformedData[mediaCategory] || [];


        items.sort((a, b) => (a.ordinate || 0) - (b.ordinate || 0));

        countLabel.innerText = `${items.length} Item${items.length !== 1 ? 's' : ''}`;
        container.innerHTML = '';

        items.forEach((item, index) => {
            const tr = document.createElement('tr');
            tr.draggable = true;
            tr.dataset.index = index;
            tr.dataset.category = mediaCategory;


            tr.ondragstart = (e) => handleDragStart(e, mediaCategory, index);
            tr.ondragover = (e) => handleDragOver(e);
            tr.ondragleave = (e) => handleDragLeave(e);
            tr.ondrop = (e) => handleDrop(e, mediaCategory, index);
            tr.ondragend = (e) => handleDragEnd(e);

            const playingLabel = (mediaCategory === 'games') ? 'Playing' : 'Watching';
            tr.innerHTML = `
                    <td><div class="item-title">${item.title}</div><div class="item-platform">${item.production}</div></td>
                    <td>${item.platform}</td>
                    <td>
                        <select class="status-select status-${item.status}-color" onchange="updateStatus('${mediaCategory}', ${index}, this.value)">
                            <option value="backlog" ${item.status === 'backlog' ? 'selected' : ''}>Backlog</option>
                            <option value="playing" ${item.status === 'playing' ? 'selected' : ''}>${playingLabel}</option>
                            <option value="completed" ${item.status === 'completed' ? 'selected' : ''}>Completed</option>
                        </select>
                    </td>
                    <td class="stars">${renderStars(mediaCategory, index, item.rating)}</td>
                    <td class="actions-cell">
                        <button class="btn-icon" onclick="toggleDropdown(event, '${mediaCategory}', ${index})">
                            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <line x1="4" y1="12" x2="20" y2="12"></line>
                                <line x1="4" y1="6" x2="20" y2="6"></line>
                                <line x1="4" y1="18" x2="20" y2="18"></line>
                            </svg>
                        </button>
                        <div class="action-dropdown" id="dropdown-${mediaCategory}-${index}">
                            <div class="action-item" onclick="startEditItem('${mediaCategory}', ${index})">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                                Edit
                            </div>
                            <div class="action-item delete" onclick="deleteItem('${mediaCategory}', ${index})">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
                                Delete
                            </div>
                        </div>
                    </td>
                `;
            container.appendChild(tr);
        });
    });
}



function handleDragStart(e, category, index) {
    draggedItemInfo = {category, index};
    e.target.classList.add('dragging');

    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/plain', index);
}

function handleDragOver(e) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    const tr = e.target.closest('tr');
    if (tr && !tr.classList.contains('dragging')) {
        tr.classList.add('drag-over');
    }
}

function handleDragLeave(e) {
    const tr = e.target.closest('tr');
    if (tr) {
        tr.classList.remove('drag-over');
    }
}

function handleDragEnd(e) {
    e.target.classList.remove('dragging');

    document.querySelectorAll('.drag-over').forEach(el => el.classList.remove('drag-over'));
}

function handleDrop(e, category, targetIndex) {
    e.preventDefault();
    const tr = e.target.closest('tr');
    if (tr) tr.classList.remove('drag-over');

    if (!draggedItemInfo || draggedItemInfo.category !== category) return;

    const sourceIndex = draggedItemInfo.index;
    if (sourceIndex === targetIndex) return;


    const list = transformedData[category];
    const [movedItem] = list.splice(sourceIndex, 1);
    list.splice(targetIndex, 0, movedItem);


    list.forEach((item, idx) => {
        item.ordinate = idx;
    });


    Promise.all(list.map(item =>
        sendHttpRequest('PUT', `http://localhost:8080/items/${item.id}`, item)
    )).then(() => {
        renderBacklog();
    }).catch(err => {
        console.error("Failed to persist new order:", err);
        renderBacklog();
    });

    draggedItemInfo = null;
}



function toggleDropdown(e, category, index) {
    e.stopPropagation();
    const dropdown = document.getElementById(`dropdown-${category}-${index}`);
    if (activeDropdown && activeDropdown !== dropdown) {
        activeDropdown.classList.remove('show');
    }
    dropdown.classList.toggle('show');
    activeDropdown = dropdown.classList.contains('show') ? dropdown : null;
}

function showAddRow(category) {
    const tbody = document.getElementById(`${category}Body`);
    if (document.getElementById(`add-row-${category}`)) return;

    if (activeDropdown) {
        activeDropdown.classList.remove('show');
        activeDropdown = null;
    }

    const playingLabel = (category === 'games') ? 'Playing' : 'Watching';
    let platformLabel = category === 'games' ? 'Platform (e.g. PC, PS5)' : 'Streaming Service';
    let productionLabel = category === 'games' ? 'Developer' : 'Studio/Network';
    const tr = document.createElement('tr');
    tr.id = `add-row-${category}`;
    tr.style.backgroundColor = 'rgba(59, 130, 246, 0.05)';
    tr.innerHTML = `
            <td><input type="text" id="new-${category}-title" class="table-input" placeholder="Title"><input type="text" id="new-${category}-prod" class="table-input" style="font-size: 0.75rem" placeholder="${productionLabel}"></td>
            <td><input type="text" id="new-${category}-platform" class="table-input" placeholder="${platformLabel}"></td>
            <td><select id="new-${category}-status" class="status-select"><option value="backlog">Backlog</option><option value="playing">${playingLabel}</option><option value="completed">Completed</option></select></td>
            <td colspan="2"><button class="btn btn-save" onclick="saveNewItem('${category}')">Save</button><button class="btn btn-cancel" onclick="renderBacklog()">Cancel</button></td>
        `;
    tbody.prepend(tr);
    document.getElementById(`new-${category}-title`).focus();
}

function startEditItem(category, index) {
    if (activeDropdown) {
        activeDropdown.classList.remove('show');
        activeDropdown = null;
    }

    const item = transformedData[category][index];
    const tbody = document.getElementById(`${category}Body`);
    const targetRow = tbody.children[index];

    const playingLabel = (category === 'games') ? 'Playing' : 'Watching';
    let platformLabel = category === 'games' ? 'Platform' : 'Streaming Service';
    let productionLabel = category === 'games' ? 'Developer' : 'Studio/Network';

    targetRow.style.backgroundColor = 'rgba(59, 130, 246, 0.05)';
    targetRow.draggable = false;
    targetRow.innerHTML = `
            <td><input type="text" id="edit-${category}-title" class="table-input" value="${item.title}"><input type="text" id="edit-${category}-prod" class="table-input" style="font-size: 0.75rem" value="${item.production}" placeholder="${productionLabel}"></td>
            <td><input type="text" id="edit-${category}-platform" class="table-input" value="${item.platform}" placeholder="${platformLabel}"></td>
            <td>
                <select id="edit-${category}-status" class="status-select">
                    <option value="backlog" ${item.status === 'backlog' ? 'selected' : ''}>Backlog</option>
                    <option value="playing" ${item.status === 'playing' ? 'selected' : ''}>${playingLabel}</option>
                    <option value="completed" ${item.status === 'completed' ? 'selected' : ''}>Completed</option>
                </select>
            </td>
            <td colspan="2">
                <button class="btn btn-save" onclick="updateItem('${category}', ${index})">Update</button>
                <button class="btn btn-cancel" onclick="renderBacklog()">Cancel</button>
            </td>
        `;
    document.getElementById(`edit-${category}-title`).focus();
}

function updateItem(category, index) {
    const title = document.getElementById(`edit-${category}-title`).value;
    const production = document.getElementById(`edit-${category}-prod`).value;
    const platform = document.getElementById(`edit-${category}-platform`).value;
    const status = document.getElementById(`edit-${category}-status`).value;

    if (!title) return;
    updateModel({title, production, platform, status}, category, index);
}

function deleteItem(category, index) {
    if (!confirm('Are you sure you want to remove this item?')) return;
    const itemToDelete = transformedData[category][index];
    sendHttpRequest('DELETE', 'http://localhost:8080/items/' + itemToDelete.id).then(() => {
        transformedData[category].splice(index, 1);

        transformedData[category].forEach((item, idx) => item.ordinate = idx);
        renderBacklog();
    }).catch((err) => console.log(err));
}

function saveNewItem(category) {
    const title = document.getElementById(`new-${category}-title`).value;
    const production = document.getElementById(`new-${category}-prod`).value;
    const platform = document.getElementById(`new-${category}-platform`).value;
    const status = document.getElementById('new-' + category + '-status').value;
    if (!title) return;


    const newOrdinate = 0;
    transformedData[category].forEach(item => item.ordinate++);

    const newItem = {
        title,
        production,
        platform,
        status,
        rating: 0,
        mediaCategory: category,
        ordinate: newOrdinate
    };
    sendHttpRequest('POST', 'http://localhost:8080/items', newItem).then((res) => {

        if (res && res.id) newItem.id = res.id;
        transformedData[category].unshift(newItem);
        renderBacklog();
    }).catch((err) => console.log(err));
}

function renderStars(type, itemIndex, currentRating) {
    let starsHtml = '';
    for (let i = 1; i <= 5; i++) {
        const isFilled = i <= currentRating;
        starsHtml += `<span class="star-item ${isFilled ? '' : 'stars-empty'}" onclick="updateRating('${type}', ${itemIndex}, ${i})">★</span>`;
    }
    return starsHtml;
}

function updateRating(category, index, newRating) {
    if (transformedData[category] && transformedData[category][index]) {
        updateModel({rating: newRating}, category, index);
    }
}

function updateStatus(category, index, newStatus) {
    if (transformedData[category] && transformedData[category][index]) {
        updateModel({status: newStatus}, category, index);
    }
}

function login() {
    const loginButton = document.getElementById('login-button');
    loginButton.innerHTML = '<span class="spinner"></span>';
    sendHttpRequest('POST', 'http://localhost:8080/auth/login', {
        Email: emailInput.value,
        Password: passwordInput.value
    }, (xhr) => {
        if(xhr.status === 200) {
            const result = xhr.response;
            accessToken = result?.accessToken || "mock-token";
            window.localStorage.setItem('accessToken', accessToken);
            document.getElementById('passErr').innerText = "";
            showDashboard();
        }
        else{
            document.getElementById('passErr').innerText = "Login failed, please try again";
            loginButton.innerText = "Sign in";
        }
    }).catch(() => {
        accessToken = "mock-token";
        showDashboard();
    });
}

async function logout() {
    window.localStorage.removeItem('accessToken');
    accessToken = null;
    emailInput.value = '';
    passwordInput.value = '';
    const loginButton = document.getElementById('login-button');
    loginButton.innerHTML = 'Sign In';
    showLogin();
}

function sendHttpRequest(method, url, data, alternateResolve) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open(method, url);
        xhr.responseType = 'json';
        xhr.setRequestHeader('Content-Type', 'application/json');
        if (accessToken) xhr.setRequestHeader('Authorization', `Bearer ${accessToken}`);
        xhr.onload = () => {
            if (alternateResolve) { 
                alternateResolve(xhr);
            }
            else { 
                if (xhr.status === 401) logout();
                resolve(xhr.response);
            }
        };
        xhr.onerror = () => reject(new Error('Network Error'));
        xhr.send(data ? JSON.stringify(data) : null);
    });
}

function getUserData() {
    sendHttpRequest('GET', 'http://localhost:8080/auth/me').then((result) => {
        const user = result || {username: "MasterChief117", firstName: "John", lastName: "117"};
        document.getElementById('display-username').innerText = user.username;
        document.getElementById('display-fullname').innerText = `${user.firstName} ${user.lastName}`;
        document.getElementById('userInitial').innerText = user.username.charAt(0).toUpperCase();
        getBacklog();
    });
}

function showDashboard() {
    getUserData();
    document.getElementById('login-view').classList.add('hidden');
    dashboardView.classList.remove('hidden');
    document.body.style.display = 'block';
}

function showLogin() {
    const loginView = document.getElementById('login-view');
    loginView.classList.remove('hidden');
    loginView.addEventListener('keypress', (e) => {
        if(e.key === 'Enter') {
            login();
        }
    });
    dashboardView.classList.add('hidden');
    document.body.style.display = 'flex';
}

function updateModel(updates, category, index) {
    const updatedItem = {
        ...transformedData[category][index],
        ...updates,
    };

    sendHttpRequest('PUT', 'http://localhost:8080/items/' + updatedItem.id, updatedItem).then(() => {
        transformedData[category][index] = updatedItem;
        renderBacklog();
    }).catch((err) => {
        console.log(err);
    });
}
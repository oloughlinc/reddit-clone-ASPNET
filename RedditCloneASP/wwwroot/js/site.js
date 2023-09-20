const postUri = 'api/posts/'
const commentUri = 'api/comments/'
const authUri = 'api/auth/login'
const refreshUri = 'api/auth/refresh'

var authToken = null;

async function onIndexLoad() {

    var loggedIn = false;
    var username = localStorage.getItem("username");

    if (username != null && username != "") {
        loggedIn = true;
    }

    if (loggedIn) {
        var userDisplay = document.getElementById("username-display");
        userDisplay.innerHTML = "Welcome u/" + username;
        userDisplay.style.removeProperty("display");
        document.getElementById("btn-logout").style.removeProperty("display");
        document.getElementById("btn-login").style.display = "none";
    } else {
        var userDisplay = document.getElementById("username-display");
        userDisplay.innerHTML = "";
        userDisplay.style.display = "none";
        document.getElementById("btn-logout").style.display = "none";
        document.getElementById("btn-login").style.removeProperty("display");
    }

    // silent sign in / receive new token on page reload
    await refreshAsync(localStorage.getItem("username"), "", refreshUri);
    console.log("auth token is:: " + authToken);
   
}

async function onCommentLoad() {

    var loggedIn = false;
    var username = localStorage.getItem("username");

    if (username != null && username != "") {
        loggedIn = true;
    }

    if (loggedIn) {
        var userDisplay = document.getElementById("username-display");
        userDisplay.innerHTML = "Welcome u/" + username;
        userDisplay.style.removeProperty("display");
        document.getElementById("btn-logout").style.removeProperty("display");
        document.getElementById("btn-login").style.display = "none";
        document.getElementById("new-post-form").style.removeProperty("display");
    } else {
        var userDisplay = document.getElementById("username-display");
        userDisplay.innerHTML = "";
        userDisplay.style.display = "none";
        document.getElementById("btn-logout").style.display = "none";
        document.getElementById("btn-login").style.removeProperty("display");
        document.getElementById("new-post-form").style.display = "none";
    }

    // silent sign in / receive new token on page reload
    await refreshAsync(localStorage.getItem("username"), "", refreshUri);
    console.log("auth token is:: " + authToken);

}

function getPosts() {
    fetch(postUri)
        .then(response => response.json())
        .then(data => _displayPosts(data))
        .catch(error => console.error('Unable to get posts.', error));
}

// To be replaced with templating once one is made.
function _displayPosts(data) {
    const body = document.getElementById('body');

    data.forEach(item => {

        var posterDiv = document.createElement("div");
        posterDiv.innerHTML += "<b>u/" + item.poster + "</b>";
        posterDiv.style.fontSize = "14px";
        body.appendChild(posterDiv);

        var div = document.createElement("div");
        div.innerHTML += '<a href="./comments.html?' + item.id + '"<b>'  + item.title + "</b></a>";
        body.appendChild(div);

        var linkDiv = document.createElement("div");
        linkDiv.innerHTML += "<a href=" + item.link + ">" + item.link + "</a>";
        body.appendChild(linkDiv);

        var contentDiv = document.createElement("div");
        contentDiv.innerHTML += item.body;
        body.appendChild(contentDiv);

        var hr = document.createElement("hr");
        body.appendChild(hr);

    })
}

function getPost() {
    var thisUrl = window.location.href;
    var postId = thisUrl.split('?')[1];

    fetch(postUri + postId)
        .then(response => response.json())
        .then(data => _displayPost(data))
        .catch(error => console.error('Unable to get post.', error));
}

// To be replaced with templating once one is made.
function _displayPost(item) {
    const body = document.getElementById('post');

    var posterDiv = document.createElement("div");
    posterDiv.innerHTML += "u/";
    posterDiv.innerHTML += item.poster;
    body.appendChild(posterDiv);

    var div = document.createElement("div");
    div.innerHTML += '<b>'  + item.title + "</b>";
    div.style.fontSize = "36px";
    body.appendChild(div);

    var linkDiv = document.createElement("div");
    linkDiv.innerHTML += "<a href=" + item.link + ">" + item.link + "</a>";
    body.appendChild(linkDiv);

    var contentDiv = document.createElement("div");
    contentDiv.innerHTML += item.body;
    body.appendChild(contentDiv);

    var space = document.createElement("div");
    space.style.height = "50px";
    body.appendChild(space);

}

function getComments() {
    var thisUrl = window.location.href;
    var commentId = thisUrl.split('?')[1];

    fetch(commentUri + commentId)
        .then(response => response.json())
        .then(data => _displayComments(data))
        .catch(error => (console.error("Unable to get comments.", error)));
}

function _displayComments(data) {
    const body = document.getElementById('body');
    var depth = 0;
    _displayCommentsRecurse(data, depth);
}

// To be replaced with templating once one is made.
function _displayCommentsRecurse(item, depth) {

    item.forEach(item => {

        var commentParentDiv = document.createElement("div");

        var posterDiv = document.createElement("div");
        posterDiv.innerHTML += "<b>u/" + item.comment.poster + "</b>";
        posterDiv.style.fontSize = "12px";
        commentParentDiv.appendChild(posterDiv);

        var div = document.createElement("div");
        div.innerHTML += item.comment.body;
        commentParentDiv.appendChild(div);

        var upsends = document.createElement("div");
        upsends.innerHTML += "<i>Upsends: " + item.comment.upsends + "</i>";
        commentParentDiv.appendChild(upsends);

        var hr = document.createElement("hr");
        commentParentDiv.appendChild(hr);

        var thisMargin = depth * 15;
        commentParentDiv.style.marginLeft = thisMargin + "px";

        body.appendChild(commentParentDiv);

        if (item.replies) {
            depth++;
            _displayCommentsRecurse(item.replies, depth);
            depth--;
        }
        
    })

}

function validate() {

    var valid = true;

    var userField = document.getElementById("username");
    var pwField = document.getElementById("password");

    if (userField.value == "") {
        userField.style.border = "1px solid red";
        valid = false;
    } else {
        userField.style.removeProperty("border");
    }

    if (pwField.value == "") {
        pwField.style.border = "1px solid red";
        valid = false;
    } else {
        pwField.style.removeProperty("border");
    }

    return valid;
}

async function authenticate(username, password, endpoint) {

    var response = await fetch(endpoint, {

        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "username": username,
            "password": password
        })
    });

    if (response.status != 200) {
        return false;
    }

    var data = await response.json();
    // store token in-memory
    authToken = data.token;
    // store username in localstorage
    localStorage.setItem("username", username);

    return true;
}

async function onLoginClick() {

    if (!validate()) return;

    var username = document.getElementById("username").value;
    var password = document.getElementById("password").value;

    var authenticated = await authenticate(username, password, authUri);

    if (authenticated) {
        location.href = "/";
        return;
    }
    document.getElementById("login-invalid").style.visibility = "visible";

}

async function onLogoutClick() {

    const authLogoutUri = "api/auth/logout";
    
    var username = localStorage.getItem("username");
    var response = await fetch(authLogoutUri, {

        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "username": username,
        })
    });

    console.log("logout response status: " + response.status);

    localStorage.removeItem("username");
    authToken = null;

    location.href = "/";
}

function showLogin() {
    const loginForm = document.getElementById("login");
    loginForm.style.visibility = "visible"
}

function hideLogin() {
    const loginForm = document.getElementById("login");
    loginForm.style.visibility = "hidden"
    document.getElementById("login-invalid").style.visibility = "hidden";
    document.getElementById("username").style.removeProperty("border");
    document.getElementById("password").style.removeProperty("border");
}

async function refreshAsync() {
    const refreshUri = "api/auth/refresh";
    var username = localStorage.getItem("username");
    if (username == null || username == "") return;
    await authenticate(username, "", refreshUri);

}
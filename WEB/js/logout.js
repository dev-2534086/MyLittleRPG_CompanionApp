// Les URLs API sont centralisées dans config.js

/**
 * Fonction principale de déconnexion
 * Appelée depuis un bouton: <button onclick="logout()" ...>
 */
async function logout() {
    const msg = document.getElementById("message");

    // Récupération de l'email du user en localStorage
    const email = localStorage.getItem("userEmail") || sessionStorage.getItem("userEmail");

    if (!email) {
        showMessage(msg, "Aucun utilisateur connecté.", "orange");
        return;
    }

    try {
        // Tentative de déconnexion
        const logoutSuccess = await attemptLogout(email);

        if (!logoutSuccess) {
            showMessage(msg, "Échec de la déconnexion.", "red");
            return;
        }

        // Nettoyage du stockage local
        clearUserData();

        showMessage(msg, "Déconnexion réussie !", "green");

        // Redirection après délai
        setTimeout(() => {
            window.location.href = "login.html";
        }, CONFIG.UI.TIMEOUT_REDIRECT);

    } catch (err) {
        console.error("Erreur dans logout():", err);
        showMessage(msg, "Erreur réseau ou serveur.", "red");
    }
}

/**
 * Envoie la requête POST à l'API pour déconnecter l'utilisateur
 * @param {string} email - Email de l'utilisateur
 * @returns {Promise<boolean>}
 */
async function attemptLogout(email) {
    try {
        const response = await fetch(
            `${CONFIG.API_BASE_URL}${CONFIG.ENDPOINTS.USERS}logout`,
            {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email })
            }
        );

        return response.ok;
    } catch (error) {
        console.error("Erreur lors de la tentative de logout:", error);
        return false;
    }
}

/**
 * Supprime les données stockées localement (email + personnage)
 */
function clearUserData() {
    localStorage.removeItem("userEmail");
    localStorage.removeItem("userCharacter");
    sessionStorage.removeItem("userEmail");
}

/**
 * Affiche un message à l'utilisateur (identique à celui du login.js)
 * @param {HTMLElement} element
 * @param {string} text
 * @param {string} color
 */
function showMessage(element, text, color) {
    if (element) {
        element.textContent = text;
        element.style.color = color;
    }
}

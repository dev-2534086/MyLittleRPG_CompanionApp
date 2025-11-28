// Les URLs API sont maintenant centralisées dans config.js

/**
 * Gestionnaire de connexion avec validation et gestion d'erreurs améliorée
 */
document.getElementById("loginForm").addEventListener("submit", async (e) => {
  e.preventDefault();

  const email = document.getElementById("email").value.trim();
  const password = document.getElementById("password").value;
  const msg = document.getElementById("message");

  // Validation des données d'entrée
  if (!email || !password) {
    showMessage(msg, "❌ Veuillez remplir tous les champs.", "red");
    return;
  }

  if (!isValidEmail(email)) {
    showMessage(msg, "❌ Format d'email invalide.", "red");
    return;
  }

  try {
    // Tentative de connexion
    const loginResult = await attemptLogin(email, password);
    if (!loginResult) {
      showMessage(msg, "❌ Identifiants incorrects.", "red");
      return;
    }

    // Stockage de l'email
    localStorage.setItem("userEmail", email);
    showMessage(msg, "✅ Connexion réussie !", "green");

    // Récupération du personnage
    await loadUserCharacter(email);

    // Redirection avec délai
    setTimeout(() => {
      window.location.href = "index.html";
    }, CONFIG.UI.TIMEOUT_REDIRECT);

  } catch (err) {
    console.error('Erreur de connexion:', err);
    showMessage(msg, "⚠️ Erreur réseau ou serveur.", "red");
  }
});

/**
 * Tente de se connecter avec l'API
 * @param {string} email - Email de l'utilisateur
 * @param {string} password - Mot de passe
 * @returns {Promise<boolean>} - Succès de la connexion
 */
async function attemptLogin(email, password) {
  try {
    const response = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.ENDPOINTS.USERS}login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password })
    });

    return response.ok;
  } catch (error) {
    console.error('Erreur lors de la tentative de connexion:', error);
    return false;
  }
}

/**
 * Charge le personnage de l'utilisateur
 * @param {string} email - Email de l'utilisateur
 * @returns {Promise<void>}
 */
async function loadUserCharacter(email) {
  try {
    const charRes = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.ENDPOINTS.CHARACTERS}${email}`, {
      method: "GET",
      headers: { "Content-Type": "application/json" },
    });

    if (!charRes.ok) {
      throw new Error("Impossible de charger les personnages");
    }

    const characters = await charRes.json();
    const myCharacter = characters.character;

    if (myCharacter) {
      localStorage.setItem("userCharacter", JSON.stringify(myCharacter));
      console.log("✅ Personnage chargé :", myCharacter);
    } else {
      console.warn("⚠️ Aucun personnage trouvé pour cet utilisateur");
    }
  } catch (error) {
    console.error('Erreur lors du chargement du personnage:', error);
  }
}

/**
 * Valide le format d'un email
 * @param {string} email - Email à valider
 * @returns {boolean} - Validité de l'email
 */
function isValidEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Affiche un message à l'utilisateur
 * @param {HTMLElement} element - Élément DOM pour le message
 * @param {string} text - Texte du message
 * @param {string} color - Couleur du message
 */
function showMessage(element, text, color) {
  if (element) {
    element.textContent = text;
    element.style.color = color;
  }
}



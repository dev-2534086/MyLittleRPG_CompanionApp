// Les URLs API sont maintenant centralisées dans config.js

/**
 * Gestionnaire d'inscription avec validation et gestion d'erreurs améliorée
 */
document.getElementById("registerForm").addEventListener("submit", async (e) => {
  e.preventDefault();

  const pseudo = document.getElementById("pseudo").value.trim();
  const email = document.getElementById("email").value.trim();
  const password = document.getElementById("password").value;
  const msg = document.getElementById("message");

  // Validation des données d'entrée
  if (!pseudo || !email || !password) {
    showMessage(msg, "❌ Veuillez remplir tous les champs.", "red");
    return;
  }

  if (!isValidEmail(email)) {
    showMessage(msg, "❌ Format d'email invalide.", "red");
    return;
  }

  if (!isValidPassword(password)) {
    showMessage(msg, "❌ Le mot de passe doit contenir au moins 6 caractères.", "red");
    return;
  }

  if (!isValidUsername(pseudo)) {
    showMessage(msg, "❌ Le pseudo doit contenir entre 3 et 20 caractères.", "red");
    return;
  }

  try {
    const result = await attemptRegistration(pseudo, email, password);
    
    if (result.success) {
      showMessage(msg, "✅ Compte créé avec succès !", "green");
      setTimeout(() => (window.location.href = "login.html"), CONFIG.UI.TIMEOUT_REGISTER);
    } else {
      showMessage(msg, `❌ Erreur : ${result.message}`, "red");
    }
  } catch (err) {
    console.error('Erreur d\'inscription:', err);
    showMessage(msg, "⚠️ Erreur réseau ou serveur.", "red");
  }
});

/**
 * Tente de créer un compte utilisateur et crée un personnage.
 * @param {string} pseudo - Pseudo de l'utilisateur
 * @param {string} email - Email de l'utilisateur
 * @param {string} password - Mot de passe
 * @returns {Promise<Object>} - Résultat de l'inscription
 */
async function attemptRegistration(pseudo, email, password) {
  try {
    const res = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.ENDPOINTS.USERS}register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username: pseudo, email, password })
    });

    let data = {};
    const contentType = res.headers.get("content-type") || "";
    if (contentType.includes("application/json")) {
      data = await res.json();
    }

    if (res.ok) {
      try {
        const charRes = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.ENDPOINTS.CHARACTERS}create/${encodeURIComponent(email)}`, {
          method: "POST"
        });

        if (charRes.ok) {
          return { success: true, message: "Compte et personnage créés avec succès" };
        } else {
          const charData = await charRes.json().catch(() => ({}));
          return { success: false, message: `Compte créé, mais échec de création du personnage: ${charData.message || "Erreur inconnue"}` };
        }

      } catch (charError) {
        console.error("Erreur lors de la création du personnage:", charError);
        return { success: false, message: "Compte créé, mais erreur réseau lors de la création du personnage" };
      }
    }

    return { success: false, message: data.message || `Code ${res.status}` };

  } catch (error) {
    console.error("Erreur lors de l'inscription:", error);
    return { success: false, message: "Erreur de connexion" };
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
 * Valide un mot de passe
 * @param {string} password - Mot de passe à valider
 * @returns {boolean} - Validité du mot de passe
 */
function isValidPassword(password) {
  return password.length >= 6;
}

/**
 * Valide un nom d'utilisateur
 * @param {string} username - Nom d'utilisateur à valider
 * @returns {boolean} - Validité du nom d'utilisateur
 */
function isValidUsername(username) {
  return username.length >= 3 && username.length <= 20;
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

/**
 * Met à jour l'interface du joueur avec validation des éléments DOM
 */
function updatePlayerUI() {
    try {
        const email = gameState.userEmail || "Inconnu";
        const charData = localStorage.getItem("userCharacter");
        const character = charData ? JSON.parse(charData) : null;

        // Mise à jour sécurisée des éléments avec validation
        updateElementText("player-name", character?.name || character?.Name || "Héros");
        updateElementText("player-email", email);
        updateElementText("player-level", character?.level ?? character?.Level ?? 1);
        updateElementText("player-xp", character?.xp ?? character?.Xp ?? 0);
        updateElementText("player-hp", character?.hp ?? character?.Hp ?? 100);
        updateElementText("player-hpmax", character?.maxHp ?? character?.MaxHp ?? 100);
        updateElementText("player-strength", character?.attack ?? character?.Attack ?? 10);
        updateElementText("player-defense", character?.defense ?? character?.Defense ?? 5);
    } catch (error) {
        console.error('Erreur lors de la mise à jour de l\'interface du joueur:', error);
    }
}

/**
 * Met à jour le texte d'un élément DOM de manière sécurisée
 * @param {string} elementId - ID de l'élément
 * @param {string|number} text - Texte à afficher
 */
function updateElementText(elementId, text) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = text;
    } else {
        console.warn(`Élément ${elementId} non trouvé`);
    }
}

// Initialisation de l'interface du joueur au chargement
window.addEventListener("DOMContentLoaded", updatePlayerUI);

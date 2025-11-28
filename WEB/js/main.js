/**
 * Initialise le jeu avec validation et gestion d'erreurs améliorée
 * @returns {Promise<void>}
 */
async function initGame() {
    // Vérification de l'authentification
    if (!gameState.userEmail) {
        window.location.href = "login.html";
        return;
    }

    try {
        // Initialisation de l'état du jeu
        gameState.currentX = CONFIG.GAME.INIT_X;
        gameState.currentY = CONFIG.GAME.INIT_Y;
        gameState.generatedTiles = {};

        // Mise à jour de l'interface utilisateur
        updatePlayerNameDisplay();

        // Nettoyage de la carte
        await clearMap();

        // Initialisation du personnage
        await initializeCharacter();

        // Génération des tuiles initiales
        await generateInitialTiles();
        
        // Mise à jour de l'interface
        updatePlayerUI();
        updateUI();
        renderMap();

    } catch (error) {
        console.error('Erreur lors de l\'initialisation du jeu:', error);
        apiService.showUserError('Erreur lors de l\'initialisation du jeu');
    }
}

/**
 * Met à jour l'affichage du nom du joueur
 */
function updatePlayerNameDisplay() {
    const playerNameElement = document.getElementById("player-name");
    if (playerNameElement) {
        playerNameElement.textContent = gameState.userEmail;
    }
}

/**
 * Nettoie la carte au démarrage
 * @returns {Promise<void>}
 */
async function clearMap() {
    try {
        await apiService.clearMap();
    } catch (error) {
        console.warn('Échec du nettoyage de la carte au démarrage:', error);
    }
}

/**
 * Initialise le personnage avec gestion d'erreurs
 * @returns {Promise<void>}
 */
async function initializeCharacter() {
    try {
        // Tentative de réparation des stats du personnage
        /*const fixed = await fixCharacterStats(gameState.userEmail);
        if (fixed) {
            localStorage.setItem('userCharacter', JSON.stringify(fixed));
        }
        */
        // Centrage du personnage
        const centered = await centerCharacter(gameState.userEmail);
        if (centered) {
            localStorage.setItem('userCharacter', JSON.stringify(centered));
        } else {
            // Récupération du personnage existant
            const existing = await fetchCharacter(gameState.userEmail);
            if (existing) {
                localStorage.setItem('userCharacter', JSON.stringify(existing));
            }
        }
        
        gameState.currentX = CONFIG.GAME.INIT_X;
        gameState.currentY = CONFIG.GAME.INIT_Y;
    } catch (error) {
        console.warn('Erreur lors de l\'initialisation du personnage:', error);
    }
}



// Initialisation du jeu au chargement de la page
window.onload = initGame;

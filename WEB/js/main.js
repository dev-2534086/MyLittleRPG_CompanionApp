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

        // Initialisation du personnage
        await initializeCharacter();

        // Génération des tuiles initiales
        await generateInitialTiles();
        
        //Chargement des quêtes
        displayQuests(gameState.userEmail);

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
        const centered = await centerMap(gameState.userEmail);
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

/**
 * Charge et affiche les quêtes dans l'onglet Quêtes
 * @param {string} email - Email du joueur connecté
 */
async function displayQuests(email) {
    const container = document.getElementById('quests-container');
    if (!container) return;

    container.innerHTML = '<p>Chargement des quêtes...</p>';

    try {
        const data = await fetchQuests(email);
        const allQuests = [
            ...(data.questTiles || []),
            ...(data.questMonsters || []),
            ...(data.questLevels || [])
        ];

        container.innerHTML = ''; // efface le message "chargement"

        allQuests.forEach(q => {
            const type = q.goalLevel
                ? 'level'
                : q.goalMonster
                ? 'monster'
                : 'tile';

            const typeInfo = {
                level:  { icon: '🧠', color: '#4CAF50', label: 'Niveau' },
                monster:{ icon: '👹', color: '#E91E63', label: 'Monstres' },
                tile:   { icon: '🗺️', color: '#2196F3', label: 'Exploration' }
            }[type];

            const questEl = document.createElement('div');
            questEl.className = 'quest-card';
            questEl.dataset.questId = q.questLevelId || q.questMonsterId || q.questTileId;
            questEl.dataset.questType = type;

            questEl.innerHTML = `
                <div class="quest-header">
                    <h4>${typeInfo.icon} ${q.title}</h4>
                    <span class="quest-type">${typeInfo.label}</span>
                </div>
                <p>${q.description}</p>
                <div class="quest-progress">
                    <div class="progress-bar-bg">
                        <div class="progress-bar-fill" id="quest-progress-${questEl.dataset.questId}"></div>
                    </div>
                    <p class="progress-text" id="quest-text-${questEl.dataset.questId}"></p>
                </div>
            `;

            container.appendChild(questEl);
        });

        // 🔄 premier update après l’affichage
        updateQuests(email);

    } catch (err) {
        console.error('Erreur lors du chargement des quêtes :', err);
        container.innerHTML = '<p style="color:red;">Impossible de charger les quêtes.</p>';
    }
}




// Initialisation du jeu au chargement de la page
window.onload = initGame;

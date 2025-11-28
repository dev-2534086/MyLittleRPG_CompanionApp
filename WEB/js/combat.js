/**
 * Simule un combat avec un monstre
 * @param {number} monsterId - ID du monstre
 * @returns {Promise<void>}
 */
async function simulateCombat(monsterId) {
    // Validation de l'authentification
    if (!gameState.userEmail) {
        apiService.showUserError('Vous devez être connecté pour simuler un combat.');
        return;
    }

    // Validation de la tuile sélectionnée
    const selectedTile = getSelectedTile();
    if (!selectedTile) {
        apiService.showUserError('Veuillez sélectionner une tuile avec un monstre.');
        return;
    }

    try {
        const combatData = {
            email: gameState.userEmail,
            monsterX: selectedTile.x,
            monsterY: selectedTile.y
        };

        const result = await apiService.simulateCombat(combatData);
        
        if (result) {
            displayCombatResult(result);
            updateQuests(gameState.userEmail);
        }
    } catch (error) {
        console.error('Erreur simulation combat:', error);
        apiService.showUserError('Erreur lors de la simulation du combat.');
    }
}

/**
 * Récupère la tuile sélectionnée avec validation
 * @returns {Object|null} - Données de la tuile sélectionnée ou null
 */
function getSelectedTile() {
    const selectedElement = document.querySelector('.tile.selected');
    if (!selectedElement) {
        console.log('Aucune tuile sélectionnée');
        return null;
    }

    const x = parseInt(selectedElement.getAttribute('data-x'));
    const y = parseInt(selectedElement.getAttribute('data-y'));
    
    if (isNaN(x) || isNaN(y)) {
        console.warn('Coordonnées de tuile invalides');
        return null;
    }

    const key = `${x},${y}`;
    const tileData = gameState.generatedTiles[key];

    if (tileData && tileData.monstre) {
        return { x, y, monster: tileData.monstre };
    }
    
    console.log('Tuile sélectionnée sans monstre');
    return null;
}

/**
 * Affiche le résultat du combat avec validation des éléments DOM
 * @param {Object} result - Résultat du combat
 */
function displayCombatResult(result) {
    const simulator = document.getElementById('combat-simulator');
    const message = document.getElementById('combat-message');
    const details = document.getElementById('combat-details');

    if (!simulator || !message || !details) {
        console.error('Éléments de combat non trouvés');
        return;
    }

    simulator.style.display = 'block';

    // Message principal avec classe de style appropriée
    const messageClass = getCombatMessageClass(result);
    message.innerHTML = `<div class="${messageClass}">${result.message || 'Combat terminé'}</div>`;

    // Détails du combat
    details.innerHTML = buildCombatDetailsHtml(result);
}

/**
 * Détermine la classe CSS pour le message de combat
 * @param {Object} result - Résultat du combat
 * @returns {string} - Classe CSS
 */
function getCombatMessageClass(result) {
    if (result.victoireJoueur) {
        return 'combat-victory';
    } else if (result.defaiteJoueur) {
        return 'combat-defeat';
        
    }
    return 'combat-neutral';
}

/**
 * Construit le HTML des détails du combat
 * @param {Object} result - Résultat du combat
 * @returns {string} - HTML des détails
 */
function buildCombatDetailsHtml(result) {
    let detailsHtml = `
        <div class="combat-stats">
            <div class="stat-item">
                <span>Nombre de tours:</span>
                <span class="turns-count">${result.nombreTours || 0}</span>
            </div>
            <div class="stat-item">
                <span>Dégâts totaux au joueur:</span>
                <span class="damage-player">${result.degatsJoueurTotal || 0}</span>
            </div>
            <div class="stat-item">
                <span>Dégâts totaux au monstre:</span>
                <span class="damage-monster">${result.degatsMonstreTotal || 0}</span>
            </div>
    `;

    // Ajouter l'XP gagnée si applicable
    if (result.experienceGagnee > 0) {
        detailsHtml += `
            <div class="stat-item">
                <span>XP gagnée:</span>
                <span class="xp-gained">+${result.experienceGagnee}</span>
            </div>
        `;
    }

    detailsHtml += '</div>';

    // Ajouter les détails des tours si disponibles
    if (result.detailsTours && Array.isArray(result.detailsTours) && result.detailsTours.length > 0) {
        detailsHtml += `
            <div class="combat-turns">
                <h4>Détails des tours:</h4>
                <div class="turns-list">
                    ${result.detailsTours.map(turn => `<div class="turn-detail">${turn}</div>`).join('')}
                </div>
            </div>
        `;
    }

    return detailsHtml;
}

/**
 * Affiche le résultat du combat dans un toast
 * @param {Object} combatResult - Résultat du combat
 */
function showCombatResult(combatResult) {
    const container = document.getElementById('toast-container');
    if (!container) {
        console.error("❌ Aucun conteneur de toasts trouvé (id='toast-container').");
        return;
    }

    // Construction du message
    let message = combatResult.message || 'Combat terminé';
    if (combatResult.victoireJoueur && combatResult.experienceGagnee > 0) {
        message;
    }

    // Création du toast
    const toast = document.createElement('div');
    toast.className = 'toast';

    // Style visuel selon le résultat
    if (combatResult.victoireJoueur) {
        toast.style.background = 'rgba(46, 204, 113, 0.9)'; // vert
    } else {
        toast.style.background = 'rgba(231, 76, 60, 0.9)'; // rouge
    }

    toast.textContent = message;
    container.appendChild(toast);

    // Suppression après 4 secondes
    setTimeout(() => {
        toast.remove();
    }, 4000);
}

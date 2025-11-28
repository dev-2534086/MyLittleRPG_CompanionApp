/**
 * Récupère et traite les tuiles pour une position donnée
 * @param {number} x - Position X
 * @param {number} y - Position Y
 * @returns {Promise<void>}
 */
async function loadTilesForPosition(x, y) {
    const data = await apiService.getTiles(x, y);
    
    if (!data) {
        // Crée une tuile "inconnue" en cas d'erreur
        const key = `${x},${y}`;
        gameState.generatedTiles[key] = { 
            description: "?", 
            estTraversable: false,
            positionX: x,
            positionY: y
        };
        return;
    }
    
    // Traiter les tuiles de la grille
    if (data.tuiles && Array.isArray(data.tuiles)) {
        data.tuiles.forEach(tuile => {
            const tileKey = `${tuile.x},${tuile.y}`;
            gameState.generatedTiles[tileKey] = {
                positionX: tuile.x,
                positionY: tuile.y,
                type: tuile.typeTuile,
                estTraversable: tuile.estAccessible,
                imageUrl: getImageUrlForType(tuile.typeTuile),
                monstre: tuile.monstre
            };
        });
    }
}

/**
 * Obtient l'URL de l'image pour un type de tuile donné
 * @param {string} type - Type de tuile
 * @returns {string} - URL de l'image
 */
function getImageUrlForType(type) {
    return CONFIG.GAME.TILE_TYPES[type] || CONFIG.GAME.TILE_TYPES.HERBE;
}


/**
 * Déplace le personnage vers une nouvelle position
 * @param {number} x - Position X
 * @param {number} y - Position Y
 * @returns {Promise<Object|null>} - Résultat du mouvement ou null
 */
async function moveCharacterToPosition(x, y) {
    const result = await apiService.moveCharacter(x, y);
    
    if (result && result.character) {
        // Mettre à jour le personnage en localStorage
        try {
            localStorage.setItem('userCharacter', JSON.stringify(result.character));
        } catch (error) {
            console.warn('Erreur lors de la sauvegarde du personnage:', error);
        }
    }
    
    return result;
}

/**
 * Récupère les informations du personnage
 * @param {string} email - Email de l'utilisateur
 * @returns {Promise<Object|null>} - Données du personnage ou null
 */
async function fetchCharacter(email) {
    return await apiService.getCharacter(email);
}


/**
 * Service API centralisé pour gérer toutes les communications avec le backend
 * Améliore la robustesse, la sécurité et la maintenabilité
 */
class ApiService {
    constructor() {
        this.baseUrl = CONFIG.API_BASE_URL;
        this.endpoints = CONFIG.ENDPOINTS;
    }

    /**
     * Méthode générique pour les appels API avec gestion d'erreurs centralisée
     * @param {string} endpoint - L'endpoint à appeler
     * @param {Object} options - Options de la requête (method, body, headers)
     * @returns {Promise<Object>} - Résultat de l'API ou null en cas d'erreur
     */
    async makeRequest(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        const defaultOptions = {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        };

        const requestOptions = { ...defaultOptions, ...options };

        try {
            const response = await fetch(url, requestOptions);
            
            // Gestion centralisée des codes d'erreur HTTP
            if (!response.ok) {
                await this.handleHttpError(response);
                return null;
            }

            // Vérification du type de contenu
            const contentType = response.headers.get('content-type') || '';
            if (contentType.includes('application/json')) {
                return await response.json();
            } else {
                return { message: await response.text() };
            }
        } catch (error) {
            console.error(`Erreur API pour ${endpoint}:`, error);
            this.showUserError('Erreur de connexion au serveur');
            return null;
        }
    }

    /**
     * Gestion centralisée des erreurs HTTP
     * @param {Response} response - Réponse HTTP
     */
    async handleHttpError(response) {
        let errorMessage = 'Erreur inconnue';
        try {
            const text = await response.text();  // lire une seule fois
            try {
                const errorData = JSON.parse(text);
                errorMessage = errorData.message || errorData.error || errorMessage;
            } catch {
                errorMessage = text || errorMessage;
            }
        } catch (e) {
            console.warn("Impossible de lire le corps de la réponse:", e);
        }
    
        switch (response.status) {
            case 401:
                this.showUserError('Session expirée. Veuillez vous reconnecter.');
                this.logout();
                break;
            case 403:
                this.showUserError('Accès refusé');
                break;
            case 404:
                this.showUserError('Ressource non trouvée');
                break;
            case 500:
                this.showUserError('Erreur serveur. Veuillez réessayer plus tard.');
                break;
            default:
                this.showUserError(errorMessage);
        }
    }
    

    /**
     * Affiche une erreur à l'utilisateur sous forme de toast
     * @param {string} message - Message d'erreur
     */
    showUserError(message) {
        const container = document.getElementById('toast-container');
        if (!container) {
            console.error("❌ Impossible d'afficher le message : aucun élément avec l'id 'toast-container' trouvé.");
            return;
        }

        const toast = document.createElement('div');
        toast.className = 'toast error-toast';
        toast.textContent = `❌ ${message}`;
        container.appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 4000);
    }


    /**
     * Déconnexion sécurisée
     */
    async logout() {
        try {
            await this.makeRequest(this.endpoints.LOGOUT, {
                method: 'POST',
                body: JSON.stringify({ email: gameState.userEmail })
            });
        } catch (error) {
            console.warn('Erreur lors de la déconnexion:', error);
        } finally {
            localStorage.removeItem('userEmail');
            localStorage.removeItem('userCharacter');
            window.location.href = 'login.html';
        }
    }

    // === MÉTHODES SPÉCIFIQUES ===

    /**
     * Récupère les tuiles pour une position donnée
     * @param {number} x - Position X
     * @param {number} y - Position Y
     * @returns {Promise<Object|null>} - Données des tuiles ou null
     */
    async getTiles(x, y) {
        return await this.makeRequest(`${this.endpoints.TILES}Grille/${x}/${y}`);
    }
    
    async getTile(x, y) {
        return await this.makeRequest(`${this.endpoints.TILES}${x}/${y}`);
    }

    /**
     * Déplace le personnage
     * @param {number} x - Position X
     * @param {number} y - Position Y
     * @returns {Promise<Object|null>} - Résultat du mouvement ou null
     */
    async moveCharacter(x, y) {
        if (!gameState.userEmail) {
            this.showUserError('Vous devez être connecté pour vous déplacer');
            return null;
        }

        return await this.makeRequest(`${this.endpoints.MOVE}${x}/${y}`, {
            method: 'POST',
            body: JSON.stringify(gameState.userEmail)
        });
    }

    /**
     * Récupère les informations du personnage
     * @param {string} email - Email de l'utilisateur
     * @returns {Promise<Object|null>} - Données du personnage ou null
     */
    async getCharacter(email) {
        if (!email) return null;
        
        const result = await this.makeRequest(`${this.endpoints.CHARACTERS}${encodeURIComponent(email)}`);
        return result?.character || null;
    }

    /**
     * Récupère les quêtes du personnage
     * @param {string} email - Email de l'utilisateur
     * @returns {Promise<Object|null>} - Données du personnage ou null
     */
    async getQuests(email) {
        if (!email) return null;
        
        const result = await this.makeRequest(`${this.endpoints.QUESTS}${encodeURIComponent(email)}`);
        return result?.quests || null;
    }

    /**
     * Simule un combat
     * @param {Object} combatData - Données du combat (email, monsterX, monsterY)
     * @returns {Promise<Object|null>} - Résultat du combat ou null
     */
    async simulateCombat(combatData) {
        return await this.makeRequest(`${this.endpoints.CHARACTERS}simulate-combat`, {
            method: 'POST',
            body: JSON.stringify(combatData)
        });
    }

    /**
     * Nettoie la carte
     * @returns {Promise<boolean>} - Succès de l'opération
     */
    async clearMap() {
        const result = await this.makeRequest(`${this.endpoints.TILES}clear`, {
            method: 'DELETE'
        });
        return result !== null;
    }
}

// Instance globale du service API
const apiService = new ApiService();

import React, { useState } from 'react';
import { createFraction } from '../services/authApi';
import { useModal } from '../hooks/useModal';
import { AlertModal } from './modals/AlertModal';

/**
 * Komponent administratora do tworzenia frakcji i generowania linków dla graczy
 */
const FractionCreator = ({ battleId }) => {
  const [fractionName, setFractionName] = useState('');
  const [playerName, setPlayerName] = useState('');
  const [fractionColor, setFractionColor] = useState('#FF0000');
  const [isLoading, setIsLoading] = useState(false);
  const [createdFractions, setCreatedFractions] = useState([]);
  const [error, setError] = useState(null);
  
  const alertModal = useModal();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      const result = await createFraction(battleId, {
        fractionName,
        playerName,
        fractionColor,
      });

      // Generuj URL dla gracza
      const playerUrl = `${window.location.origin}/battles/${battleId}/simulator?token=${result.authToken}&fractionId=${result.fractionId}`;

      setCreatedFractions([
        ...createdFractions,
        {
          fractionName,
          playerName,
          fractionColor,
          fractionId: result.fractionId,
          authToken: result.authToken,
          playerUrl,
        },
      ]);

      // Wyczyść formularz
      setFractionName('');
      setPlayerName('');
      setFractionColor('#FF0000');
    } catch (err) {
      setError(err.response?.data?.message || 'Błąd podczas tworzenia frakcji');
      console.error('Error creating fraction:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const copyToClipboard = async (text) => {
    try {
      // Próbuj użyć nowoczesnego API
      if (navigator.clipboard && navigator.clipboard.writeText) {
        await navigator.clipboard.writeText(text);
      } else {
        // Fallback dla starszych przeglądarek lub HTTP
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
      }
      alertModal.openModal({
        title: 'Sukces',
        message: 'Skopiowano do schowka!',
        variant: 'success'
      });
    } catch (error) {
      console.error('Failed to copy:', error);
      alertModal.openModal({
        title: 'Błąd',
        message: 'Nie udało się skopiować. Tekst: ' + text,
        variant: 'error'
      });
    }
  };

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h2 className="text-3xl font-bold mb-6">Tworzenie Frakcji</h2>

      {/* Formularz */}
      <form onSubmit={handleSubmit} className="bg-white shadow-md rounded px-8 pt-6 pb-8 mb-6">
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="fractionName">
            Nazwa Frakcji
          </label>
          <input
            id="fractionName"
            type="text"
            value={fractionName}
            onChange={(e) => setFractionName(e.target.value)}
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            placeholder="np. Imperium Galaktyczne"
            required
          />
        </div>

        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="playerName">
            Nazwa Gracza
          </label>
          <input
            id="playerName"
            type="text"
            value={playerName}
            onChange={(e) => setPlayerName(e.target.value)}
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            placeholder="np. Jan Kowalski"
            required
          />
        </div>

        <div className="mb-6">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="fractionColor">
            Kolor Frakcji
          </label>
          <div className="flex items-center gap-4">
            <input
              id="fractionColor"
              type="color"
              value={fractionColor}
              onChange={(e) => setFractionColor(e.target.value)}
              className="h-10 w-20 cursor-pointer"
            />
            <input
              type="text"
              value={fractionColor}
              onChange={(e) => setFractionColor(e.target.value)}
              className="shadow appearance-none border rounded py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              placeholder="#FF0000"
            />
          </div>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        <button
          type="submit"
          disabled={isLoading}
          className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline disabled:opacity-50"
        >
          {isLoading ? 'Tworzenie...' : 'Utwórz Frakcję'}
        </button>
      </form>

      {/* Lista utworzonych frakcji */}
      {createdFractions.length > 0 && (
        <div className="bg-white shadow-md rounded px-8 pt-6 pb-8">
          <h3 className="text-2xl font-bold mb-4">Utworzone Frakcje</h3>
          {createdFractions.map((fraction, index) => (
            <div key={index} className="mb-6 p-4 border rounded" style={{ borderColor: fraction.fractionColor }}>
              <div className="flex items-center gap-3 mb-2">
                <div
                  className="w-6 h-6 rounded"
                  style={{ backgroundColor: fraction.fractionColor }}
                ></div>
                <h4 className="text-xl font-bold">{fraction.fractionName}</h4>
                <span className="text-gray-600">({fraction.playerName})</span>
              </div>

              <div className="mb-2">
                <p className="text-sm text-gray-600 font-semibold">Fraction ID:</p>
                <div className="flex items-center gap-2">
                  <code className="text-xs bg-gray-100 p-2 rounded flex-1">{fraction.fractionId}</code>
                  <button
                    onClick={() => copyToClipboard(fraction.fractionId)}
                    className="text-blue-500 hover:text-blue-700 text-sm"
                  >
                    Kopiuj
                  </button>
                </div>
              </div>

              <div className="mb-2">
                <p className="text-sm text-gray-600 font-semibold">Auth Token:</p>
                <div className="flex items-center gap-2">
                  <code className="text-xs bg-gray-100 p-2 rounded flex-1">{fraction.authToken}</code>
                  <button
                    onClick={() => copyToClipboard(fraction.authToken)}
                    className="text-blue-500 hover:text-blue-700 text-sm"
                  >
                    Kopiuj
                  </button>
                </div>
              </div>

              <div>
                <p className="text-sm text-gray-600 font-semibold mb-1">Link dla gracza:</p>
                <div className="flex items-center gap-2">
                  <input
                    type="text"
                    value={fraction.playerUrl}
                    readOnly
                    className="text-xs bg-blue-50 p-2 rounded flex-1 border border-blue-200"
                  />
                  <button
                    onClick={() => copyToClipboard(fraction.playerUrl)}
                    className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded text-sm"
                  >
                    Kopiuj Link
                  </button>
                </div>
                <p className="text-xs text-gray-500 mt-1">
                  Wyślij ten link graczowi aby mógł dołączyć do gry
                </p>
              </div>
            </div>
          ))}
        </div>
      )}

      <AlertModal
        isOpen={alertModal.isOpen}
        onClose={alertModal.closeModal}
        title={alertModal.modalData.title}
        message={alertModal.modalData.message}
        variant={alertModal.modalData.variant}
      />
    </div>
  );
};

export default FractionCreator;

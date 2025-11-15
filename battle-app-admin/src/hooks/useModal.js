import { useState, useCallback } from 'react';

/**
 * Hook do zarządzania stanem modali
 */
export const useModal = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [modalData, setModalData] = useState({});

  const openModal = useCallback((data = {}) => {
    setModalData(data);
    setIsOpen(true);
  }, []);

  const closeModal = useCallback(() => {
    setIsOpen(false);
    setModalData({});
  }, []);

  return {
    isOpen,
    modalData,
    openModal,
    closeModal,
  };
};

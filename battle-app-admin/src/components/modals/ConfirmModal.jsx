import {
  DialogRoot,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogBody,
  DialogFooter,
  DialogCloseTrigger,
  DialogBackdrop,
  DialogPositioner,
} from '@chakra-ui/react';
import { Button, HStack } from '@chakra-ui/react';

/**
 * Modal do potwierdzania akcji (zamiennik confirm)
 */
export const ConfirmModal = ({ 
  isOpen, 
  onClose, 
  onConfirm, 
  title = 'Potwierdzenie', 
  message,
  confirmText = 'OK',
  cancelText = 'Cancel',
  colorScheme = 'blue'
}) => {
  const handleConfirm = () => {
    onConfirm();
    onClose();
  };

  return (
    <DialogRoot 
      open={isOpen} 
      onOpenChange={(e) => !e.open && onClose()} 
      size="md"
    >
      <DialogBackdrop />
      <DialogPositioner>
        <DialogContent bg="white" color="black">
          <DialogHeader borderBottom="1px solid" borderColor="gray.200">
            <DialogTitle color="black">{title}</DialogTitle>
          </DialogHeader>
          <DialogCloseTrigger />
          <DialogBody py={4}>
            <p>{message}</p>
          </DialogBody>
          <DialogFooter borderTop="1px solid" borderColor="gray.200" pt={4}>
            <HStack gap={3}>
              <Button variant="outline" onClick={onClose}>
                {cancelText}
              </Button>
              <Button colorPalette={colorScheme} onClick={handleConfirm}>
                {confirmText}
              </Button>
            </HStack>
          </DialogFooter>
        </DialogContent>
      </DialogPositioner>
    </DialogRoot>
  );
};

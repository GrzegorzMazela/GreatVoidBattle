import {
  DialogRoot,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogBody,
  DialogFooter,
  DialogCloseTrigger,
  DialogActionTrigger,
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
    <DialogRoot open={isOpen} onOpenChange={(e) => !e.open && onClose()} size="md">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>
        <DialogCloseTrigger />
        <DialogBody>
          <p>{message}</p>
        </DialogBody>
        <DialogFooter>
          <HStack spacing={3}>
            <DialogActionTrigger asChild>
              <Button variant="outline" onClick={onClose}>
                {cancelText}
              </Button>
            </DialogActionTrigger>
            <Button colorScheme={colorScheme} onClick={handleConfirm}>
              {confirmText}
            </Button>
          </HStack>
        </DialogFooter>
      </DialogContent>
    </DialogRoot>
  );
};

import { useForm, useWatch, useFieldArray } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { updateShip } from '../../services/api';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  DialogRoot, 
  DialogContent, 
  DialogHeader, 
  DialogTitle, 
  DialogBody, 
  DialogFooter,
  DialogBackdrop,
  DialogCloseTrigger,
  VStack, 
  Field, 
  Input, 
  NativeSelectRoot, 
  NativeSelectField, 
  Button, 
  HStack, 
  createToaster, 
  Toaster,
  Box,
  Text,
  Heading
} from '@chakra-ui/react';
import { ShipTypes } from '../../types/dto';
import { useEffect } from 'react';

const WeaponTypes = ['Missile', 'Laser', 'PointDefense'];

const getModuleCountForShipType = (type) => {
  switch(type) {
    case 'Corvette': return 1;
    case 'Destroyer': return 2;
    case 'Cruiser': return 4;
    case 'Battleship': return 8;
    case 'SuperBattleship': return 12;
    case 'OrbitalFort': return 2;
    default: return 0;
  }
};

const schema = z.object({
  name: z.string().min(1),
  type: z.enum(ShipTypes),
  positionX: z.coerce.number().min(0),
  positionY: z.coerce.number().min(0),
  modules: z.array(z.object({
    weaponTypes: z.array(z.string()).length(3)
  }))
});

const toaster = createToaster({
  placement: 'top-end',
  duration: 3000,
});

export default function ShipEditDialog({ ship, battleId, fractionId, onClose }) {
  const qc = useQueryClient();
  
  const { register, handleSubmit, control } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      name: ship.name,
      type: ship.type,
      positionX: ship.x,
      positionY: ship.y,
      modules: ship.modules?.map(m => ({
        weaponTypes: m.weaponTypes || ['Missile', 'Laser', 'PointDefense']
      })) || []
    }
  });

  const { fields, replace } = useFieldArray({
    control,
    name: 'modules'
  });

  const shipType = useWatch({ control, name: 'type' });

  useEffect(() => {
    const moduleCount = getModuleCountForShipType(shipType);
    const currentModules = fields.length;
    
    if (currentModules !== moduleCount) {
      const newModules = Array.from({ length: moduleCount }, (_, i) => 
        fields[i] || { weaponTypes: ['Missile', 'Laser', 'PointDefense'] }
      );
      replace(newModules);
    }
  }, [shipType, replace, fields]);

  const mutation = useMutation({
    mutationFn: (payload) => updateShip(battleId, fractionId, ship.shipId, payload),
    onSuccess: () => {
      qc.invalidateQueries(['ships', battleId, fractionId]);
      qc.invalidateQueries(['battle', battleId]);
      toaster.create({ title: 'Ship updated', type: 'success' });
      onClose();
    }
  });

  return (
    <>
      <Toaster toaster={toaster} />
      <DialogRoot 
        open={true} 
        size="xl"
        blockScrollOnMount={false}
        preserveScrollBarGap
      >
        <DialogBackdrop />
        <DialogContent maxH="90vh" overflowY="auto">
          <DialogCloseTrigger onClick={onClose} />
          <DialogHeader>
            <DialogTitle>Edit Ship</DialogTitle>
          </DialogHeader>
          <DialogBody pb="6">
            <VStack as="form" id="edit-ship-form" align="stretch" gap="4" onSubmit={handleSubmit((v) => mutation.mutate(v))}>
              <Field.Root>
                <Field.Label>Name</Field.Label>
                <Input {...register('name')} />
              </Field.Root>
              <Field.Root>
                <Field.Label>Type</Field.Label>
                <NativeSelectRoot>
                  <NativeSelectField {...register('type')}>
                    {ShipTypes.map(t => <option key={t} value={t}>{t}</option>)}
                  </NativeSelectField>
                </NativeSelectRoot>
              </Field.Root>
              <HStack>
                <Field.Root>
                  <Field.Label>Position X</Field.Label>
                  <Input type="number" {...register('positionX')} />
                </Field.Root>
                <Field.Root>
                  <Field.Label>Position Y</Field.Label>
                  <Input type="number" {...register('positionY')} />
                </Field.Root>
              </HStack>
              
              {fields.length > 0 && (
                <Box mt="4" p="4" borderWidth="1px" borderRadius="md">
                  <Heading size="sm" mb="3">Modules ({fields.length} required)</Heading>
                  <VStack align="stretch" gap="4">
                    {fields.map((field, moduleIndex) => (
                      <Box key={field.id} p="3" borderWidth="1px" borderRadius="md" bg="gray.50">
                        <Text fontWeight="bold" mb="2">Module {moduleIndex + 1}</Text>
                        <HStack gap="2">
                          {[0, 1, 2].map((slotIndex) => (
                            <Field.Root key={slotIndex} flex="1">
                              <Field.Label>Slot {slotIndex + 1}</Field.Label>
                              <NativeSelectRoot>
                                <NativeSelectField {...register(`modules.${moduleIndex}.weaponTypes.${slotIndex}`)}>
                                  {WeaponTypes.map(wt => <option key={wt} value={wt}>{wt}</option>)}
                                </NativeSelectField>
                              </NativeSelectRoot>
                            </Field.Root>
                          ))}
                        </HStack>
                      </Box>
                    ))}
                  </VStack>
                </Box>
              )}
            </VStack>
          </DialogBody>
          <DialogFooter>
            <HStack>
              <Button variant="outline" onClick={onClose}>Cancel</Button>
              <Button type="submit" form="edit-ship-form" colorScheme="blue" isLoading={mutation.isPending}>
                Save Changes
              </Button>
            </HStack>
          </DialogFooter>
        </DialogContent>
      </DialogRoot>
    </>
  );
}

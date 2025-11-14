import { useParams, useNavigate } from 'react-router-dom';
import { useForm, useWatch, useFieldArray } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { createShip, updateShip, getShip } from '../../services/api';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { Box, Heading, VStack, Field, Input, NativeSelectRoot, NativeSelectField, Button, HStack, createToaster, Toaster, Text, Spinner } from '@chakra-ui/react';
import { ShipTypes, emptyShipPayload } from '../../types/dto';
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

export default function ShipForm() {
  const { battleId, fractionId, shipId } = useParams();
  const isEditMode = !!shipId;
  
  const { data: existingShip, isLoading: loadingShip } = useQuery({
    queryKey: ['ship', battleId, fractionId, shipId],
    queryFn: () => getShip(battleId, fractionId, shipId),
    enabled: isEditMode
  });

  const { register, handleSubmit, control, reset } = useForm({
    resolver: zodResolver(schema),
    defaultValues: emptyShipPayload()
  });
  
  const { fields, replace } = useFieldArray({
    control,
    name: 'modules'
  });
  const qc = useQueryClient();
  const nav = useNavigate();

  const shipType = useWatch({ control, name: 'type' });

  // Load existing ship data when in edit mode
  useEffect(() => {
    if (existingShip && isEditMode) {
      reset({
        name: existingShip.name,
        type: existingShip.type,
        positionX: existingShip.x,
        positionY: existingShip.y,
        modules: existingShip.modules?.map(m => ({
          weaponTypes: m.weaponTypes || ['Missile', 'Laser', 'PointDefense']
        })) || []
      });
    }
  }, [existingShip, isEditMode, reset]);

  useEffect(() => {
    const moduleCount = getModuleCountForShipType(shipType);
    const currentModules = fields.length;
    
    // Only auto-generate modules if not in edit mode or if module count changed
    if (!isEditMode || (isEditMode && currentModules !== moduleCount && currentModules === 0)) {
      const newModules = Array.from({ length: moduleCount }, () => ({
        weaponTypes: ['Missile', 'Laser', 'PointDefense']
      }));
      replace(newModules);
    }
  }, [shipType, replace, fields.length, isEditMode]);

  const mutation = useMutation({
    mutationFn: (payload) => isEditMode 
      ? updateShip(battleId, fractionId, shipId, payload)
      : createShip(battleId, fractionId, payload),
    onSuccess: () => {
      qc.invalidateQueries(['ships', battleId, fractionId]);
      qc.invalidateQueries(['battle', battleId]);
      toaster.create({ title: isEditMode ? 'Ship updated' : 'Ship created', type: 'success' });
      nav(`/admin/${battleId}/fractions/${fractionId}/ships`);
    }
  });

  if (isEditMode && loadingShip) return <Spinner />;

  return (
    <>
      <Toaster toaster={toaster} />
      <Box>
        <Heading size="md" mb="4">{isEditMode ? 'Edit Ship' : 'Add Ship'}</Heading>
        <VStack as="form" align="stretch" spacing="4" onSubmit={handleSubmit((v) => mutation.mutate(v))}>
          <Field.Root><Field.Label>Name</Field.Label><Input {...register('name')} /></Field.Root>
          <Field.Root>
            <Field.Label>Type</Field.Label>
            <NativeSelectRoot>
              <NativeSelectField {...register('type')}>
                {ShipTypes.map(t => <option key={t} value={t}>{t}</option>)}
              </NativeSelectField>
            </NativeSelectRoot>
          </Field.Root>
          <HStack>
            <Field.Root><Field.Label>Position X</Field.Label><Input type="number" {...register('positionX')} /></Field.Root>
            <Field.Root><Field.Label>Position Y</Field.Label><Input type="number" {...register('positionY')} /></Field.Root>
          </HStack>
          
          {fields.length > 0 && (
            <Box mt="4" p="4" borderWidth="1px" borderRadius="md">
              <Heading size="sm" mb="3">Modules ({fields.length} required)</Heading>
              <VStack align="stretch" spacing="4">
                {fields.map((field, moduleIndex) => (
                  <Box key={field.id} p="3" borderWidth="1px" borderRadius="md" bg="gray.50">
                    <Text fontWeight="bold" mb="2">Module {moduleIndex + 1}</Text>
                    <HStack spacing="2">
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

          <Button type="submit" colorScheme="green" isLoading={mutation.isPending}>
            {isEditMode ? 'Update' : 'Save'}
          </Button>
          <Button variant="outline" onClick={() => nav(`/admin/${battleId}/fractions/${fractionId}/ships`)}>
            Cancel
          </Button>
        </VStack>
      </Box>
    </>
  );
}

import { useParams, useNavigate } from 'react-router-dom';
import { useForm, useWatch, useFieldArray } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { createShip, updateShip, getShip } from '../../services/api';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { Box, Heading, VStack, Field, Input, NativeSelectRoot, NativeSelectField, Button, HStack, Text, Spinner } from '@chakra-ui/react';
import { ShipTypes, ShipDefaultStats, WeaponDefaults, emptyShipPayload } from '../../types/dto';
import { useEffect } from 'react';
import { useNotification } from '../../contexts/NotificationContext';

const WeaponTypes = ['Missile', 'Laser', 'PointDefense'];

const getModuleCountForShipType = (type) => {
  return ShipDefaultStats[type]?.modules || 0;
};

const schema = z.object({
  name: z.string().min(1),
  type: z.enum(ShipTypes),
  positionX: z.coerce.number().min(0),
  positionY: z.coerce.number().min(0),
  speed: z.coerce.number().min(0).optional(),
  hitPoints: z.coerce.number().min(1).optional(),
  shields: z.coerce.number().min(0).optional(),
  armor: z.coerce.number().min(0).optional(),
  laserMaxRange: z.coerce.number().min(0).optional(),
  laserDamage: z.coerce.number().min(0).optional(),
  missileMaxRange: z.coerce.number().min(0).optional(),
  missileEffectiveRange: z.coerce.number().min(0).optional(),
  missileDamage: z.coerce.number().min(0).optional(),
  missileSpeed: z.coerce.number().min(0).optional(),
  modules: z.array(z.object({
    weaponTypes: z.array(z.string()).length(3)
  }))
});

export default function ShipForm() {
  const { showSuccess } = useNotification();
  const { battleId, fractionId, shipId } = useParams();
  const isEditMode = !!shipId;
  
  const { data: existingShip, isLoading: loadingShip } = useQuery({
    queryKey: ['ship', battleId, fractionId, shipId],
    queryFn: () => getShip(battleId, fractionId, shipId),
    enabled: isEditMode
  });

  const { register, handleSubmit, control, reset, setValue } = useForm({
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

  // Dla nowego statku: ustaw domyślne wartości broni w UI przy pierwszym montażu
  useEffect(() => {
    if (!isEditMode) {
      setValue('laserMaxRange', WeaponDefaults.laserMaxRange);
      setValue('laserDamage', WeaponDefaults.laserDamage);
      setValue('missileMaxRange', WeaponDefaults.missileMaxRange);
      setValue('missileEffectiveRange', WeaponDefaults.missileEffectiveRange);
      setValue('missileDamage', WeaponDefaults.missileDamage);
      setValue('missileSpeed', WeaponDefaults.missileSpeed);
    }
  }, [isEditMode, setValue]);

  // Load existing ship data when in edit mode
  useEffect(() => {
    if (existingShip && isEditMode) {
      reset({
        name: existingShip.name,
        type: existingShip.type,
        positionX: existingShip.x,
        positionY: existingShip.y,
        speed: existingShip.speed,
        hitPoints: existingShip.hitPoints,
        shields: existingShip.shields,
        armor: existingShip.armor,
        laserMaxRange: existingShip.laserMaxRange ?? WeaponDefaults.laserMaxRange,
        laserDamage: existingShip.laserDamage ?? WeaponDefaults.laserDamage,
        missileMaxRange: existingShip.missileMaxRange ?? WeaponDefaults.missileMaxRange,
        missileEffectiveRange: existingShip.missileEffectiveRange ?? WeaponDefaults.missileEffectiveRange,
        missileDamage: existingShip.missileDamage ?? WeaponDefaults.missileDamage,
        missileSpeed: existingShip.missileSpeed ?? WeaponDefaults.missileSpeed,
        modules: existingShip.modules?.map(m => ({
          weaponTypes: m.weaponTypes || ['Missile', 'Laser', 'PointDefense']
        })) || []
      });
    }
  }, [existingShip, isEditMode, reset]);

  useEffect(() => {
    const defaults = ShipDefaultStats[shipType];
    if (!defaults) return;
    
    const moduleCount = defaults.modules;
    const currentModules = fields.length;
    
    // Update modules when ship type changes
    if (currentModules !== moduleCount) {
      const newModules = Array.from({ length: moduleCount }, () => ({
        weaponTypes: ['Missile', 'Laser', 'PointDefense']
      }));
      replace(newModules);
    }
    
    // Update default stats when type changes (only for new ships)
    if (!isEditMode && defaults) {
      setValue('speed', defaults.speed);
      setValue('hitPoints', defaults.hitPoints);
      setValue('shields', defaults.shields);
      setValue('armor', defaults.armor);
      setValue('laserMaxRange', defaults.laserMaxRange);
      setValue('laserDamage', defaults.laserDamage);
      setValue('missileMaxRange', defaults.missileMaxRange);
      setValue('missileEffectiveRange', defaults.missileEffectiveRange);
      setValue('missileDamage', defaults.missileDamage);
      setValue('missileSpeed', defaults.missileSpeed);
    }
  }, [shipType, replace, fields.length, isEditMode, setValue]);

  const mutation = useMutation({
    mutationFn: (payload) => isEditMode 
      ? updateShip(battleId, fractionId, shipId, payload)
      : createShip(battleId, fractionId, payload),
    onSuccess: () => {
      qc.invalidateQueries(['ships', battleId, fractionId]);
      qc.invalidateQueries(['battle', battleId]);
      showSuccess(isEditMode ? 'Statek zaktualizowany' : 'Statek utworzony');
      nav(`/pustka-admin-panel/${battleId}/fractions/${fractionId}/ships`);
    }
  });

  if (isEditMode && loadingShip) return <Spinner />;

  return (
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
          
          <Box mt="4" p="4" borderWidth="1px" borderRadius="md" bg="gray.50">
            <Text fontWeight="bold" mb="3">Parametry statku (domyślne dla typu)</Text>
            <HStack spacing="4" flexWrap="wrap">
              <Field.Root flex="1" minW="100px">
                <Field.Label>Prędkość</Field.Label>
                <Input type="number" {...register('speed')} />
              </Field.Root>
              <Field.Root flex="1" minW="100px">
                <Field.Label>HP</Field.Label>
                <Input type="number" {...register('hitPoints')} />
              </Field.Root>
              <Field.Root flex="1" minW="100px">
                <Field.Label>Tarcze</Field.Label>
                <Input type="number" {...register('shields')} />
              </Field.Root>
              <Field.Root flex="1" minW="100px">
                <Field.Label>Pancerz</Field.Label>
                <Input type="number" {...register('armor')} />
              </Field.Root>
            </HStack>
          </Box>
          <Box mt="4" p="4" borderWidth="1px" borderRadius="md" bg="blue.50">
            <Text fontWeight="bold" mb="3">Broń – zasięg i obrażenia (domyślnie: laser 15/30, rakiety 55/35/20/15)</Text>
            <HStack spacing="4" flexWrap="wrap">
              <Field.Root flex="1" minW="100px">
                <Field.Label>Zasięg lasera</Field.Label>
                <Input type="number" {...register('laserMaxRange')} placeholder={String(WeaponDefaults.laserMaxRange)} />
              </Field.Root>
              <Field.Root flex="1" minW="100px">
                <Field.Label>Obr. lasera</Field.Label>
                <Input type="number" {...register('laserDamage')} placeholder={String(WeaponDefaults.laserDamage)} />
              </Field.Root>
              <Field.Root flex="1" minW="100px">
                <Field.Label>Zasięg rakiet</Field.Label>
                <Input type="number" {...register('missileMaxRange')} placeholder={String(WeaponDefaults.missileMaxRange)} />
              </Field.Root>
              <Field.Root flex="1" minW="100px">
                <Field.Label>Zasięg efektyw. rakiet</Field.Label>
                <Input type="number" {...register('missileEffectiveRange')} placeholder={String(WeaponDefaults.missileEffectiveRange)} />
              </Field.Root>
              <Field.Root flex="1" minW="100px">
                <Field.Label>Obr. rakiety</Field.Label>
                <Input type="number" {...register('missileDamage')} placeholder={String(WeaponDefaults.missileDamage)} />
              </Field.Root>
              <Field.Root flex="1" minW="100px">
                <Field.Label>Prędk. rakiety</Field.Label>
                <Input type="number" {...register('missileSpeed')} placeholder={String(WeaponDefaults.missileSpeed)} />
              </Field.Root>
            </HStack>
          </Box>
          
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
          <Button variant="outline" onClick={() => nav(`/pustka-admin-panel/${battleId}/fractions/${fractionId}/ships`)}>
            Cancel
          </Button>
        </VStack>
    </Box>
  );
}
